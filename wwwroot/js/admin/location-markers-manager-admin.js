// LocationMarkers class - quản lý marker địa điểm trên bản đồ Leaflet, tích hợp kiểm tra vùng bằng Turf.js và gọi API
// Yêu cầu: Leaflet, Turf.js, L.Icon.Pulse.js đã được load
class LocationMarkers {

    setLocationList(locationListInstance) {
        this.locationList = locationListInstance;
    }
    constructor(map, geojsonLayer, options = {}) {
        // Map và GeoJSON layer quản lý polygon tỉnh
        this.map = map || null;
        this.geojsonLayer = geojsonLayer || null;

        // Danh sách marker hiện trên bản đồ và dữ liệu location
        this.markers = [];
        this.locations = [];

        // Trạng thái tỉnh được chọn và phiên bản location (để sync update)
        this.currentProvinceGid = null;
        this.currentProvinceName = null;
        this.locationsVersion = 0;
        this.provinceInteractionState = 0;
        // Cấu hình mặc định, có thể ghi đè qua options
        this.config = {
            apiEndpoints: {
                locations: '/api/locations',
                checkLocation: '/api/locations/check',
                addLocation: '/api/location'
            },
            routes: {
                editLocation: options.editLocationRoute || '/Admin/Locations/Edit',
                viewPosts: options.viewPostsRoute || '/Spaceshare/Index'
            },
            markerConfig: {
                iconSize: [16, 16],
                color: "red",
                fillColor: "red",
                heartbeat: 1.5
            },
            ...options
        };

        // Callback integration
        this.onLocationAdded = options.onLocationAdded || null;
        this.onLocationDeleted = options.onLocationDeleted || null;
        this.onMarkersUpdated = options.onMarkersUpdated || null;

        // Bind các hàm xử lý event đúng context
        this.handleMapClick = this.handleMapClick.bind(this);

        // Khởi tạo listener bản đồ
        this.initializeMapListener();
    }

    // --- Khởi tạo hoặc reset listener click trên map ---
    // Thay đổi hàm cập nhật trạng thái province
    setProvinceInteractionState(state) {
        console.log(`🎯 State change: ${this.provinceInteractionState} -> ${state}`);
        this.provinceInteractionState = state;

        // Thêm thông báo rõ ràng cho từng trạng thái
        const stateMessages = {
            0: 'Trạng thái ban đầu - chưa chọn tỉnh hoặc zoom chưa đủ',
            1: 'Sẵn sàng thêm địa điểm - click lần đầu',
            2: 'Đã click lần đầu - click lần hai để xác nhận'
        };

        console.log('State description:', stateMessages[state] || 'Unknown state');
    }
    initializeMapListener() {
        if (!this.map) {
            console.error('LocationMarkers: Map instance is required');
            return;
        }

        this.map.off('click', this.handleMapClick);
        this.map.on('click', this.handleMapClick);

        // Tự động chuyển sang trạng thái 1 khi zoom đủ sâu và có tỉnh được chọn
        this.map.on('zoomend', () => {
            const zoomLevel = this.map.getZoom();
            const requiredZoom = 10;

            // Chỉ khi có tỉnh được chọn, zoom đủ lớn và đang ở trạng thái 0
            if (this.currentProvinceGid && zoomLevel >= requiredZoom && this.provinceInteractionState === 0) {
                this.setProvinceInteractionState(1);
                this.showSuccessNotification('Bạn có thể click trên bản đồ để thêm địa điểm mới.');
            }
        });

        console.log('LocationMarkers: Map click & zoom listener initialized');
    }



    // --- Cập nhật GeoJSON layer (polygon tỉnh) ---
    setGeoJsonLayer(geojsonLayer) {
        this.geojsonLayer = geojsonLayer;
        console.log('LocationMarkers: GeoJSON layer updated');
    }

    // --- Tải marker mới theo provinceGid ---
    // Fixed loadMarkers method - only set state to 1 when actually loading a new province
    async loadMarkers(provinceGid, provinceName = null, version = 0) {
        try {
            this.clearMarkers(false);  // Don't reset state here

            if (!provinceGid) {
                console.warn('LocationMarkers: No province GID provided');
                return;
            }

            // Check if this is a new province selection or just a refresh
            const isNewProvince = this.currentProvinceGid !== provinceGid;

            this.currentProvinceGid = provinceGid;
            this.currentProvinceName = provinceName;
            this.locationsVersion = version;

            // Only set state to 1 if this is a NEW province selection
            // Don't change state if we're just refreshing markers for the same province
            if (isNewProvince) {
                console.log('Loading markers for NEW province, setting state to 1');
                this.setProvinceInteractionState(1);
            } else {
                console.log('Refreshing markers for SAME province, keeping current state:', this.provinceInteractionState);
            }

            const response = await fetch(`${this.config.apiEndpoints.locations}?gid=${provinceGid}`);

            if (!response.ok) throw new Error(`Failed to fetch locations: ${response.status} ${response.statusText}`);

            const data = await response.json();

            if (!data.data || !Array.isArray(data.data)) {
                console.warn('LocationMarkers: Invalid location data received');
                this.locations = [];
                return;
            }

            this.locations = data.data;

            await this.createMarkersFromLocations(this.locations);

            if (this.onMarkersUpdated) {
                this.onMarkersUpdated({
                    provinceGid,
                    locationsCount: this.locations.length,
                    version: this.locationsVersion
                });
            }

            console.log(`LocationMarkers: Loaded ${this.locations.length} markers`);

        } catch (error) {
            console.error('LocationMarkers: Error loading markers:', error);
            this.showErrorNotification('Không thể tải danh sách địa điểm. Vui lòng thử lại.');
        }
    }

    // --- Tạo marker Leaflet từ danh sách locations ---
    async createMarkersFromLocations(locations) {
        for (const location of locations) {
            if (!this.isValidLocationData(location)) {
                console.warn('LocationMarkers: Invalid location data:', location);
                continue;
            }

            try {
                const marker = await this.createLocationMarker(location);
                if (marker) {
                    marker.addTo(this.map);
                    this.markers.push(marker);
                }
            } catch (error) {
                console.error('LocationMarkers: Error creating marker:', location._id, error);
            }
        }
    }

    // --- Kiểm tra dữ liệu location hợp lệ ---
    isValidLocationData(location) {
        return (
            location &&
            location.coordinates &&
            typeof location.coordinates.lat === 'number' &&
            typeof location.coordinates.lng === 'number' &&
            !isNaN(location.coordinates.lat) &&
            !isNaN(location.coordinates.lng) &&
            Math.abs(location.coordinates.lat) <= 90 &&
            Math.abs(location.coordinates.lng) <= 180
        );
    }

    // --- Tạo marker Leaflet với icon pulse và popup tùy chỉnh ---
    async createLocationMarker(location) {
        try {
            const pulseIcon = L.icon.pulse({
                iconSize: this.config.markerConfig.iconSize,
                color: this.config.markerConfig.color,
                fillColor: this.config.markerConfig.fillColor,
                heartbeat: this.config.markerConfig.heartbeat
            });

            const marker = L.marker([location.coordinates.lat, location.coordinates.lng], { icon: pulseIcon });

            const popupContent = this.createPopupContent(location);
            marker.bindPopup(popupContent, { maxWidth: 300, className: 'location-marker-popup' });

            marker.on('popupopen', () => this.handlePopupOpen(location));

            return marker;

        } catch (error) {
            console.error('LocationMarkers: Error creating marker:', error);
            return null;
        }
    }

    // --- Tạo nội dung popup an toàn ---
    createPopupContent(location) {
        const name = location.name || 'Địa điểm chưa đặt tên';
        const description = location.description || '';
        const coords = `${location.coordinates.lat.toFixed(6)}, ${location.coordinates.lng.toFixed(6)}`;

        return `
      <div style="font-family: Arial, sans-serif; min-width: 200px;">
        <h3 style="margin: 0 0 8px; font-weight: 600; color: #2c3e50;">${this.escapeHtml(name)}</h3>
        ${description ? `<p style="margin: 0 0 12px; color: #6c757d; line-height: 1.4;">${this.escapeHtml(description)}</p>` : ''}
        <small style="color: #868e96; display: block; margin-bottom: 12px;">📍 ${coords}</small>
        <div style="display: flex; gap: 8px; justify-content: center;">
          <button class="view-posts-btn" data-location-id="${location._id}" style="${this.buttonStyle('#007bff')}">📄 Xem bài viết</button>
          <button class="edit-location-btn" data-location-id="${location._id}" style="${this.buttonStyle('#28a745')}">✏️ Chỉnh sửa</button>
        </div>
      </div>
    `;
    }

    // --- Style nút để reuse ---
    buttonStyle(bgColor) {
        return `
      padding: 6px 12px;
      font-size: 12px;
      background-color: ${bgColor};
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      transition: background-color 0.2s ease;
    `;
    }

    // --- Xử lý sự kiện mở popup, gán listener cho button ---
    handlePopupOpen(location) {
        setTimeout(() => {
            // Xử lý nút Xem bài viết
            const viewBtn = document.querySelector(`.view-posts-btn[data-location-id="${location._id}"]`);
            if (viewBtn) {
                const newViewBtn = viewBtn.cloneNode(true);
                viewBtn.parentNode.replaceChild(newViewBtn, viewBtn);
                newViewBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    window.open(`${this.config.routes.viewPosts}?location_id=${location._id}`, '_blank');
                });
            }

            // Xử lý nút Chỉnh sửa
            const editBtn = document.querySelector(`.edit-location-btn[data-location-id="${location._id}"]`);
            if (editBtn) {
                const newEditBtn = editBtn.cloneNode(true);
                editBtn.parentNode.replaceChild(newEditBtn, editBtn);
                newEditBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    window.open(`${this.config.routes.editLocation}/${location._id}`, '_blank');
                });
            }
        }, 50);
    }

    // --- Xử lý sự kiện click bản đồ để thêm location mới ---



    // --- Kiểm tra tọa độ hợp lệ ---
    isValidCoordinates(lat, lng) {
        return (
            typeof lat === 'number' &&
            typeof lng === 'number' &&
            !isNaN(lat) &&
            !isNaN(lng) &&
            Math.abs(lat) <= 90 &&
            Math.abs(lng) <= 180
        );
    }

    // Sửa lại phần xử lý trong handleMapClick method

    // Sửa lại phần xử lý trong handleMapClick method
    async handleMapClick(e) {
        console.log('=== MAP CLICK DEBUG ===');
        console.log('Current province interaction state:', this.provinceInteractionState);
        console.log('Current province GID:', this.currentProvinceGid);
        console.log('Click coordinates:', e.latlng);

        try {
            // Kiểm tra có tỉnh được chọn chưa
            if (!this.currentProvinceGid) {
                this.showWarningNotification('Vui lòng chọn tỉnh/thành phố trước khi thêm địa điểm.');
                return;
            }

            // Logic kiểm tra trạng thái 1 -> 2
            if (this.provinceInteractionState === 1) {
                console.log('🔄 Changing state from 1 to 2');
                this.setProvinceInteractionState(2);
                this.showWarningNotification('Nhấn lần nữa để thêm địa điểm.');
                console.log('⏹️ RETURN: Waiting for second click');
                return;
            }

            // Chỉ thực hiện thêm địa điểm khi ở trạng thái 2
            if (this.provinceInteractionState !== 2) {
                console.log('❌ Not in state 2, ignoring click. Current state:', this.provinceInteractionState);
                return;
            }

            const { lat, lng } = e.latlng;

            if (!this.isValidCoordinates(lat, lng)) {
                this.showErrorNotification('Tọa độ không hợp lệ.');
                return;
            }

            // Kiểm tra điểm click có nằm trong tỉnh được chọn không
            if (!this.isLatLngInSelectedProvince(lat, lng)) {
                this.showWarningNotification(`Bạn chỉ được phép thêm địa điểm trong khu vực ${this.currentProvinceName || 'tỉnh đã chọn'}!`);
                return;
            }

            // Kiểm tra xem vị trí này đã có trong database chưa
            const existingLocation = await this.checkLocationInDatabase(lat, lng, this.currentProvinceGid);

            if (!existingLocation) {
                // Nếu chưa có, hỏi người dùng có muốn thêm không
                const isConfirmed = window.confirm('Vị trí này chưa có trong cơ sở dữ liệu. Bạn có muốn thêm địa điểm không?');

                if (isConfirmed) {
                    try {
                        console.log('🔄 Adding new location to database...');

                        // Thêm location mới vào database
                        const newLocation = await this.addLocationToDatabase(lat, lng, this.currentProvinceGid);

                        console.log('✅ New location added:', newLocation);

                        this.showSuccessNotification('Địa điểm đã được thêm thành công! Đang chuyển hướng...');

                        // Reset về trạng thái 1 sau khi thêm thành công
                        this.setProvinceInteractionState(1);

                        // Gọi callback nếu có
                        if (this.onLocationAdded) {
                            this.onLocationAdded(newLocation);
                        }

                        // Kiểm tra và chuyển hướng đến trang edit
                        // Lấy ID từ response (có thể là _id hoặc id)
                        const locationId = newLocation._id || newLocation.id;

                        if (this.config.routes.editLocation && newLocation && locationId) {
                            console.log('🔗 Redirecting to edit page:', `${this.config.routes.editLocation}/${locationId}`);

                            // Thêm delay nhỏ để user thấy thông báo thành công
                            setTimeout(() => {
                                // Sử dụng window.location.href thay vì window.location để đảm bảo chuyển hướng
                                window.location.href = `${this.config.routes.editLocation}/${locationId}`;
                            }, 1000);
                        } else {
                            console.warn('⚠️ Cannot redirect: missing editLocation route or location ID');
                            console.log('Edit route:', this.config.routes.editLocation);
                            console.log('New location _id:', newLocation?._id);
                            console.log('New location id:', newLocation?.id);

                            // Refresh markers nếu không thể chuyển hướng
                            await this.loadMarkers(this.currentProvinceGid, this.currentProvinceName, this.locationsVersion + 1);
                        }

                    } catch (error) {
                        console.error('❌ Error adding new location:', error);
                        this.showErrorNotification('Có lỗi xảy ra khi thêm địa điểm mới. Vui lòng thử lại.');
                        // Reset về trạng thái 1 khi có lỗi
                        this.setProvinceInteractionState(1);
                    }
                } else {
                    console.log('🚫 User cancelled adding new location');
                    // Reset về trạng thái 1 khi user hủy
                    this.setProvinceInteractionState(1);
                }
            } else {
                // Nếu đã có location tại vị trí này
                this.showWarningNotification('Vị trí này đã có trong cơ sở dữ liệu.');
                // Reset về trạng thái 1
                this.setProvinceInteractionState(1);

                // Có thể mở popup của marker tương ứng nếu muốn
                const existingMarker = this.markers.find(marker => {
                    const markerLatLng = marker.getLatLng();
                    const distance = this.calculateDistance(
                        markerLatLng.lat, markerLatLng.lng,
                        existingLocation.coordinates.lat, existingLocation.coordinates.lng
                    );
                    return distance < 0.0001; // tolerance ~11m
                });

                if (existingMarker) {
                    existingMarker.openPopup();
                }
            }

        } catch (error) {
            console.error('❌ Error handling map click:', error);
            this.showErrorNotification('Có lỗi xảy ra khi xử lý click trên bản đồ.');
            // Reset về trạng thái 1 khi có lỗi
            this.setProvinceInteractionState(1);
        }
    }

    // --- Kiểm tra điểm nằm trong polygon tỉnh hiện tại (cải tiến) ---
    // Hàm kiểm tra điểm trong polygon (ray casting), polygonCoords dạng [[lng, lat], [lng, lat], ...]
    isPointInPolygon(lat, lng, polygonCoords) {
        let inside = false;
        let j = polygonCoords.length - 1;
        for (let i = 0; i < polygonCoords.length; i++) {
            const xi = polygonCoords[i][0]; // lng
            const yi = polygonCoords[i][1]; // lat
            const xj = polygonCoords[j][0];
            const yj = polygonCoords[j][1];

            if (((yi > lat) !== (yj > lat)) &&
                (lng < (xj - xi) * (lat - yi) / (yj - yi) + xi)) {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }

    isLatLngInSelectedProvince(lat, lng) {
        try {
            if (!this.geojsonLayer || !this.currentProvinceGid) {
                console.warn('LocationMarkers: Missing geojsonLayer or currentProvinceGid');
                return false;
            }

            if (!this.isValidCoordinates(lat, lng)) {
                console.warn('LocationMarkers: Invalid coordinates for boundary check');
                return false;
            }

            let isInside = false;

            this.geojsonLayer.eachLayer(layer => {
                if (isInside) return; // dừng nếu đã tìm thấy

                const feature = layer.feature;
                if (!feature || !feature.properties) return;

                // So sánh gid, ép kiểu số
                const featureGid = Number(feature.properties.gid);
                const currentGid = Number(this.currentProvinceGid);

                if (featureGid !== currentGid) return;

                const geomType = feature.geometry?.type;
                if (geomType !== "Polygon" && geomType !== "MultiPolygon") return;

                // Lấy tọa độ polygon
                let polygonsCoords = [];

                if (geomType === "Polygon") {
                    polygonsCoords = feature.geometry.coordinates; // Mảng các vòng (linear rings)
                } else if (geomType === "MultiPolygon") {
                    // flatten 1 cấp vì MultiPolygon là mảng mảng polygon
                    polygonsCoords = feature.geometry.coordinates.flat();
                }

                // Duyệt từng polygon (vòng linear ring đầu tiên là ngoại vi)
                for (const polygonCoords of polygonsCoords) {
                    if (this.isPointInPolygon(lat, lng, polygonCoords)) {
                        isInside = true;
                        break;
                    }
                }
            });

            return isInside;

        } catch (error) {
            console.error('LocationMarkers: Boundary check error:', error);
            return false;
        }
    }


    // --- Helper function: Tính khoảng cách giữa 2 điểm (để tìm marker gần nhất) ---
    calculateDistance(lat1, lng1, lat2, lng2) {
        const R = 6371; // Bán kính Trái Đất (km)
        const dLat = this.toRadians(lat2 - lat1);
        const dLng = this.toRadians(lng2 - lng1);
        const a =
            Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.cos(this.toRadians(lat1)) * Math.cos(this.toRadians(lat2)) *
            Math.sin(dLng / 2) * Math.sin(dLng / 2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        return R * c; // Khoảng cách tính bằng km
    }

    // --- Helper function: Chuyển độ sang radian ---
    toRadians(degrees) {
        return degrees * (Math.PI / 180);
    }

    // --- Kiểm tra điểm nằm trong polygon tỉnh ---


    // --- Kiểm tra tồn tại location tương tự trong database (dựa trên khoảng cách/tolerance) ---
    async checkLocationInDatabase(lat, lng, provinceGid) {
        try {
            const tolerance = 0.0001; // khoảng ~11m, backend cần xử lý tham số này

            const url = `${this.config.apiEndpoints.checkLocation}?gid=${provinceGid}&lat=${lat}&lng=${lng}&tolerance=${tolerance}`;
            const res = await fetch(url);

            if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);

            const data = await res.json();

            return data.exists ? data.location : null;

        } catch (error) {
            console.error('LocationMarkers: Error checking location in DB:', error);
            return null; // Nếu lỗi, cho phép thêm mới
        }
    }

    // --- Thêm location mới lên DB ---
    async addLocationToDatabase(lat, lng, provinceGid) {
        try {
            const locationData = {
                coordinates: { lat, lng },
                provinceGid,
                name: '',
                description: '',
                createdAt: new Date().toISOString()
            };

            console.log('📤 Sending location data to API:', locationData);

            const response = await fetch(this.config.apiEndpoints.addLocation, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(locationData)
            });

            console.log('📥 API Response status:', response.status);

            const result = await response.json();
            console.log('📥 API Response data:', result);

            if (!response.ok) {
                throw new Error(result.error || `HTTP ${response.status}: ${response.statusText}`);
            }

            // Kiểm tra cấu trúc response và trả về location data
            if (result.data) {
                return result.data;
            } else if (result._id || result.id) {
                // Trường hợp API trả về trực tiếp location object
                return result;
            } else {
                console.warn('⚠️ Unexpected API response structure:', result);
                return result;
            }

        } catch (error) {
            console.error('❌ Error adding location to database:', error);
            throw error;
        }
    }

    // --- Xóa toàn bộ marker và reset state ---
    clearMarkers(resetState = true) {
        try {
            this.markers.forEach(marker => {
                if (marker && this.map.hasLayer(marker)) this.map.removeLayer(marker);
            });
            this.markers = [];
            this.locations = [];

            if (resetState) {
                // Chỉ reset khi cần, ví dụ khi thoát tỉnh hoặc gọi explicit
                this.provinceInteractionState = 0;
                console.log('LocationMarkers: provinceInteractionState reset to 0');
            }

            console.log('LocationMarkers: Markers cleared');
        } catch (error) {
            console.error('LocationMarkers: Error clearing markers:', error);
        }
    }


    // --- Cập nhật lại markers nếu version thay đổi ---
    updateMarkers(locationsVersion) {
        if (this.currentProvinceGid && locationsVersion !== this.locationsVersion) {
            console.log(`LocationMarkers: Updating markers for version ${locationsVersion}`);
            this.loadMarkers(this.currentProvinceGid, this.currentProvinceName, locationsVersion);
        }
    }

    // --- Xóa marker theo locationId ---
    removeMarkerByLocationId(locationId) {
        try {
            const index = this.markers.findIndex(marker => {
                const content = marker.getPopup()?.getContent() || '';
                return content.includes(`data-location-id="${locationId}"`);
            });

            if (index === -1) return false;

            const marker = this.markers[index];
            if (this.map.hasLayer(marker)) this.map.removeLayer(marker);
            this.markers.splice(index, 1);

            this.locations = this.locations.filter(loc => loc._id !== locationId);

            console.log(`LocationMarkers: Removed marker for location ${locationId}`);
            return true;

        } catch (error) {
            console.error('LocationMarkers: Error removing marker:', error);
            return false;
        }
    }

    // --- Thông báo người dùng (success, warning, error) ---
    showSuccessNotification(message) { this.showNotification(message, 'success'); }
    showWarningNotification(message) { this.showNotification(message, 'warning'); }
    showErrorNotification(message) { this.showNotification(message, 'error'); }

    showNotification(message, type = 'info') {
        const notif = document.createElement('div');

        const styles = {
            success: { bg: '#d4edda', color: '#155724', border: '#c3e6cb' },
            warning: { bg: '#fff3cd', color: '#856404', border: '#ffeaa7' },
            error: { bg: '#f8d7da', color: '#721c24', border: '#f5c6cb' },
            info: { bg: '#d1ecf1', color: '#0c5460', border: '#bee5eb' }
        };

        const style = styles[type] || styles.info;

        Object.assign(notif.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '12px 20px',
            backgroundColor: style.bg,
            color: style.color,
            border: `1px solid ${style.border}`,
            borderRadius: '8px',
            boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
            zIndex: '10000',
            maxWidth: '350px',
            fontSize: '14px',
            fontFamily: 'Arial, sans-serif',
            opacity: '1',
            transition: 'opacity 0.3s ease, transform 0.3s ease'
        });

        notif.textContent = message;
        document.body.appendChild(notif);

        setTimeout(() => {
            notif.style.opacity = '0';
            notif.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notif.parentNode) notif.parentNode.removeChild(notif);
            }, 300);
        }, 5000);
    }

    // --- Escape HTML để tránh XSS ---
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // --- Lấy trạng thái hiện tại ---
    getCurrentState() {
        return {
            currentProvinceGid: this.currentProvinceGid,
            currentProvinceName: this.currentProvinceName,
            markersCount: this.markers.length,
            locationsCount: this.locations.length,
            locationsVersion: this.locationsVersion,
            hasGeojsonLayer: !!this.geojsonLayer,
            hasMapInstance: !!this.map
        };
    }

    // --- Dọn dẹp khi huỷ component ---
    destroy() {
        try {
            if (this.map) this.map.off('click', this.handleMapClick);
            this.clearMarkers();

            this.currentProvinceGid = null;
            this.currentProvinceName = null;
            this.locationsVersion = 0;
            this.geojsonLayer = null;

            console.log('LocationMarkers: Destroyed successfully');
        } catch (error) {
            console.error('LocationMarkers: Error during destroy:', error);
        }
    }
}
