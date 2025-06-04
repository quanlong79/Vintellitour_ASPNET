// Enhanced LocationList class with improved functionality and modern design
class LocationList {
    constructor(options) {
        this.container = document.getElementById(options.containerId);
        this.onLocationDeletedCallback = options.onLocationDeletedCallback;
        this.onLocationAddedCallback = options.onLocationAddedCallback;
        this.editUrlBase ='/Admin/Locations/Edit';
        this.provinceDetailUrlBase = options.provinceDetailUrlBase || '/provinces/';

        this.locations = [];
        this.filteredLocations = [];
        this.currentProvinceGid = null;
        this.currentProvinceName = null;
        this.isVisible = false;
        this.locationsVersion = 0;

        if (!this.container) {
            console.error(`LocationList: Container with ID '${options.containerId}' not found.`);
        }
    }

    show(provinceGid, provinceName, locationsVersion = 0) {
        if (!this.container) return;

        this.currentProvinceGid = provinceGid;
        this.currentProvinceName = provinceName || 'Tỉnh đã chọn';
        this.locationsVersion = locationsVersion;
        this.isVisible = true;

        this.renderContainer();
        this.fetchLocations(provinceGid);
    }

    hide() {
        if (!this.container) return;

        this.container.innerHTML = '';
        this.container.style.display = 'none';
        this.currentProvinceGid = null;
        this.currentProvinceName = null;
        this.locations = [];
        this.filteredLocations = [];
        this.isVisible = false;
        this.locationsVersion = 0;
    }

    updateLocations(locationsVersion) {
        if (this.isVisible && this.currentProvinceGid && locationsVersion !== this.locationsVersion) {
            this.locationsVersion = locationsVersion;
            this.fetchLocations(this.currentProvinceGid);
        }
    }

    renderContainer() {
        this.container.innerHTML = '';
        this.container.style.display = 'block';

        // Modern glassmorphism container styling
        Object.assign(this.container.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            width: '380px',
            maxHeight: 'calc(100vh - 40px)',
            background: 'linear-gradient(145deg, rgba(255,255,255,0.95) 0%, rgba(248,250,252,0.95) 100%)',
            backdropFilter: 'blur(20px)',
            borderRadius: '20px',
            boxShadow: '0 20px 40px rgba(0, 0, 0, 0.1), 0 8px 25px rgba(0, 0, 0, 0.08)',
            border: '1px solid rgba(255, 255, 255, 0.3)',
            overflow: 'hidden',
            zIndex: '1000',
            fontFamily: '"Segoe UI", Tahoma, Geneva, Verdana, sans-serif',
            animation: 'slideInRight 0.3s ease-out'
        });

        // Add CSS animations
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideInRight {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes fadeIn {
                from { opacity: 0; transform: translateY(10px); }
                to { opacity: 1; transform: translateY(0); }
            }
            @keyframes pulse {
                0%, 100% { transform: scale(1); }
                50% { transform: scale(1.05); }
            }
            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }
            .location-item {
                animation: fadeIn 0.4s ease-out;
            }
            .location-item:nth-child(odd) {
                animation-delay: 0.1s;
            }
            .location-item:nth-child(even) {
                animation-delay: 0.2s;
            }
        `;
        document.head.appendChild(style);

        // Enhanced header with gradient
        const headerDiv = document.createElement('div');
        Object.assign(headerDiv.style, {
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            padding: '24px 24px 20px',
            borderRadius: '20px 20px 0 0',
            position: 'relative',
            overflow: 'hidden'
        });

        // Add decorative element
        const decorElement = document.createElement('div');
        Object.assign(decorElement.style, {
            position: 'absolute',
            top: '-50%',
            right: '-20%',
            width: '100px',
            height: '100px',
            background: 'rgba(255, 255, 255, 0.1)',
            borderRadius: '50%',
            pointerEvents: 'none'
        });
        headerDiv.appendChild(decorElement);

        const headerContent = document.createElement('div');
        Object.assign(headerContent.style, {
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            position: 'relative',
            zIndex: '2'
        });

        const title = document.createElement('h2');
        title.innerHTML = `<span style="display: block; font-size: 14px; font-weight: 400; margin-bottom: 4px; opacity: 0.9;">Danh sách địa điểm</span>${this.currentProvinceName}`;
        Object.assign(title.style, {
            fontSize: '20px',
            fontWeight: '700',
            margin: '0',
            color: 'white',
            textShadow: '0 2px 4px rgba(0,0,0,0.1)',
            lineHeight: '1.2'
        });
        headerContent.appendChild(title);

        // Modern close button
        const closeButton = document.createElement('button');
        closeButton.innerHTML = '✕';
        Object.assign(closeButton.style, {
            background: 'rgba(255, 255, 255, 0.2)',
            border: 'none',
            color: 'white',
            width: '32px',
            height: '32px',
            borderRadius: '50%',
            cursor: 'pointer',
            fontSize: '16px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            transition: 'all 0.2s ease',
            backdropFilter: 'blur(10px)'
        });
        closeButton.addEventListener('mouseenter', () => {
            closeButton.style.background = 'rgba(255, 255, 255, 0.3)';
            closeButton.style.transform = 'scale(1.1)';
        });
        closeButton.addEventListener('mouseleave', () => {
            closeButton.style.background = 'rgba(255, 255, 255, 0.2)';
            closeButton.style.transform = 'scale(1)';
        });
        closeButton.addEventListener('click', () => this.hide());
        headerContent.appendChild(closeButton);

        headerDiv.appendChild(headerContent);

        // Province info link
        const provinceInfoLink = document.createElement('a');
        provinceInfoLink.href = `${this.provinceDetailUrlBase}${this.currentProvinceGid}`;
        provinceInfoLink.innerHTML = '🔗 Thông tin chi tiết tỉnh';
        provinceInfoLink.target = '_blank';
        Object.assign(provinceInfoLink.style, {
            display: 'inline-block',
            fontSize: '13px',
            color: 'rgba(255, 255, 255, 0.9)',
            textDecoration: 'none',
            padding: '8px 16px',
            background: 'rgba(255, 255, 255, 0.15)',
            borderRadius: '20px',
            marginTop: '12px',
            transition: 'all 0.2s ease',
            backdropFilter: 'blur(10px)',
            position: 'relative',
            zIndex: '2'
        });
        provinceInfoLink.addEventListener('mouseenter', () => {
            provinceInfoLink.style.background = 'rgba(255, 255, 255, 0.25)';
            provinceInfoLink.style.transform = 'translateY(-1px)';
        });
        provinceInfoLink.addEventListener('mouseleave', () => {
            provinceInfoLink.style.background = 'rgba(255, 255, 255, 0.15)';
            provinceInfoLink.style.transform = 'translateY(0)';
        });
        headerDiv.appendChild(provinceInfoLink);

        this.container.appendChild(headerDiv);

        // Enhanced search section
        const searchDiv = document.createElement('div');
        searchDiv.style.padding = '20px 24px 16px';

        const searchWrapper = document.createElement('div');
        Object.assign(searchWrapper.style, {
            position: 'relative',
            display: 'flex',
            alignItems: 'center'
        });

        const searchIcon = document.createElement('div');
        searchIcon.innerHTML = '🔍';
        Object.assign(searchIcon.style, {
            position: 'absolute',
            left: '12px',
            zIndex: '1',
            fontSize: '16px',
            opacity: '0.6'
        });

        const searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.placeholder = 'Tìm kiếm địa điểm theo tên...';
        Object.assign(searchInput.style, {
            width: '100%',
            padding: '14px 16px 14px 44px',
            border: '2px solid rgba(102, 126, 234, 0.1)',
            borderRadius: '12px',
            fontSize: '14px',
            outline: 'none',
            transition: 'all 0.3s ease',
            boxSizing: 'border-box',
            background: 'rgba(255, 255, 255, 0.8)',
            backdropFilter: 'blur(10px)'
        });

        searchInput.addEventListener('focus', () => {
            searchInput.style.borderColor = 'rgba(102, 126, 234, 0.5)';
            searchInput.style.boxShadow = '0 0 0 4px rgba(102, 126, 234, 0.1)';
            searchInput.style.background = 'rgba(255, 255, 255, 0.95)';
        });
        searchInput.addEventListener('blur', () => {
            searchInput.style.borderColor = 'rgba(102, 126, 234, 0.1)';
            searchInput.style.boxShadow = 'none';
            searchInput.style.background = 'rgba(255, 255, 255, 0.8)';
        });
        searchInput.addEventListener('input', (e) => this.handleSearch(e.target.value));

        searchWrapper.appendChild(searchIcon);
        searchWrapper.appendChild(searchInput);
        searchDiv.appendChild(searchWrapper);
        this.container.appendChild(searchDiv);

        // Enhanced list container
        this.listElement = document.createElement('div');
        Object.assign(this.listElement.style, {
            padding: '0 24px 24px',
            maxHeight: 'calc(100vh - 280px)',
            overflowY: 'auto',
            overflowX: 'hidden'
        });

        // Custom scrollbar
        const scrollStyle = document.createElement('style');
        scrollStyle.textContent = `
            .location-list-container::-webkit-scrollbar {
                width: 8px;
            }
            .location-list-container::-webkit-scrollbar-track {
                background: rgba(0, 0, 0, 0.05);
                border-radius: 4px;
            }
            .location-list-container::-webkit-scrollbar-thumb {
                background: linear-gradient(145deg, #667eea, #764ba2);
                border-radius: 4px;
            }
            .location-list-container::-webkit-scrollbar-thumb:hover {
                background: linear-gradient(145deg, #764ba2, #667eea);
            }
        `;
        document.head.appendChild(scrollStyle);
        this.listElement.className = 'location-list-container';

        this.container.appendChild(this.listElement);
    }
    fetchLocations(provinceGid) {
        if (!this.listElement) return;

        this.showLoadingState();

        fetch(`/api/locations?gid=${provinceGid}`)
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}: ${res.statusText}`);
                return res.json();
            })
            .then(data => {
                console.log("Fetched locations data:", data.data);  // <-- Thêm dòng này
                this.locations = data.data || [];
                this.filteredLocations = [...this.locations];
                this.renderList();
            })
            .catch(err => {
                console.error("Lỗi khi tải địa điểm:", err);
                this.showErrorState(err.message);
            });
    }


    showLoadingState() {
        if (!this.listElement) return;

        this.listElement.innerHTML = `
            <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 60px 20px; color: #667eea;">
                <div style="animation: spin 1s linear infinite; width: 32px; height: 32px; border: 3px solid rgba(102, 126, 234, 0.2); border-top: 3px solid #667eea; border-radius: 50%; margin-bottom: 16px;"></div>
                <div style="font-weight: 500; font-size: 16px; margin-bottom: 8px;">Đang tải địa điểm</div>
                <div style="font-size: 14px; opacity: 0.7;">Vui lòng chờ một chút...</div>
            </div>
        `;
    }

    showErrorState(message) {
        if (!this.listElement) return;

        this.listElement.innerHTML = `
            <div style="text-align: center; padding: 60px 20px;">
                <div style="font-size: 64px; margin-bottom: 20px; filter: grayscale(1); opacity: 0.6;">⚠️</div>
                <div style="font-weight: 600; margin-bottom: 12px; color: #e74c3c; font-size: 18px;">Không thể tải danh sách</div>
                <div style="font-size: 14px; color: #95a5a6; margin-bottom: 24px; line-height: 1.5;">${message}</div>
                <button onclick="location.reload()" style="
                    padding: 12px 24px; 
                    background: linear-gradient(135deg, #e74c3c, #c0392b); 
                    color: white; 
                    border: none; 
                    border-radius: 8px; 
                    cursor: pointer; 
                    font-weight: 500;
                    transition: all 0.2s ease;
                    box-shadow: 0 4px 12px rgba(231, 76, 60, 0.3);
                " onmouseenter="this.style.transform='translateY(-2px)'" onmouseleave="this.style.transform='translateY(0)'">
                    🔄 Thử lại
                </button>
            </div>
        `;
    }

    renderList() {
        if (!this.listElement) return;

        this.listElement.innerHTML = '';

        if (this.filteredLocations.length === 0) {
            this.listElement.innerHTML = `
                <div style="text-align: center; padding: 60px 20px; color: #95a5a6;">
                    <div style="font-size: 64px; margin-bottom: 20px; opacity: 0.6;">📍</div>
                    <div style="font-size: 18px; font-weight: 500; margin-bottom: 8px;">Không có địa điểm nào</div>
                    <div style="font-size: 14px; opacity: 0.8;">Thử tìm kiếm với từ khóa khác</div>
                </div>
            `;
            return;
        }

        // Stats header
        const statsDiv = document.createElement('div');
        Object.assign(statsDiv.style, {
            padding: '12px 16px',
            background: 'linear-gradient(135deg, rgba(102, 126, 234, 0.1), rgba(118, 75, 162, 0.1))',
            borderRadius: '12px',
            marginBottom: '20px',
            border: '1px solid rgba(102, 126, 234, 0.2)'
        });
        statsDiv.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: center;">
                <span style="font-size: 14px; color: #667eea; font-weight: 500;">
                    📊 Tìm thấy ${this.filteredLocations.length} địa điểm
                </span>
                <span style="font-size: 12px; color: #95a5a6;">
                    Tổng: ${this.locations.length}
                </span>
            </div>
        `;
        this.listElement.appendChild(statsDiv);

        const listContainer = document.createElement('div');

        this.filteredLocations.forEach((loc, index) => {
            const listItem = this.createLocationItem(loc, index);
            listContainer.appendChild(listItem);
        });

        this.listElement.appendChild(listContainer);
    }

    createLocationItem(loc, index) {
        const listItem = document.createElement('div');
        listItem.className = 'location-item';
        Object.assign(listItem.style, {
            padding: '20px',
            marginBottom: '16px',
            background: 'linear-gradient(145deg, rgba(255,255,255,0.9) 0%, rgba(248,250,252,0.9) 100%)',
            borderRadius: '16px',
            border: '1px solid rgba(102, 126, 234, 0.1)',
            transition: 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)',
            cursor: 'default',
            position: 'relative',
            overflow: 'hidden',
            backdropFilter: 'blur(10px)'
        });

        // Add hover shimmer effect
        const shimmer = document.createElement('div');
        Object.assign(shimmer.style, {
            position: 'absolute',
            top: '0',
            left: '-100%',
            width: '100%',
            height: '100%',
            background: 'linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent)',
            transition: 'left 0.5s ease',
            pointerEvents: 'none'
        });
        listItem.appendChild(shimmer);

        listItem.addEventListener('mouseenter', () => {
            listItem.style.transform = 'translateY(-4px) scale(1.02)';
            listItem.style.boxShadow = '0 12px 32px rgba(102, 126, 234, 0.15), 0 4px 12px rgba(0, 0, 0, 0.1)';
            listItem.style.borderColor = 'rgba(102, 126, 234, 0.3)';
            shimmer.style.left = '100%';
        });
        listItem.addEventListener('mouseleave', () => {
            listItem.style.transform = 'translateY(0) scale(1)';
            listItem.style.boxShadow = 'none';
            listItem.style.borderColor = 'rgba(102, 126, 234, 0.1)';
            shimmer.style.left = '-100%';
        });

        // Content wrapper
        const contentWrapper = document.createElement('div');
        contentWrapper.style.position = 'relative';
        contentWrapper.style.zIndex = '1';

        // Location name with icon
        const nameElement = document.createElement('h3');
        nameElement.innerHTML = `<span style="margin-right: 8px;">📍</span>${loc.name || 'Địa điểm chưa đặt tên'}`;
        Object.assign(nameElement.style, {
            fontSize: '18px',
            fontWeight: '600',
            margin: '0 0 12px 0',
            color: '#2c3e50',
            lineHeight: '1.3'
        });
        contentWrapper.appendChild(nameElement);

        // Description with fade effect
        if (loc.description) {
            const descElement = document.createElement('p');
            descElement.textContent = loc.description;
            Object.assign(descElement.style, {
                fontSize: '14px',
                color: '#6c757d',
                margin: '0 0 16px 0',
                lineHeight: '1.5',
                background: 'rgba(108, 117, 125, 0.05)',
                padding: '12px',
                borderRadius: '8px',
                borderLeft: '3px solid #667eea'
            });
            contentWrapper.appendChild(descElement);
        }

        // Coordinates with copy functionality
        if (loc.coordinates && loc.coordinates.lat && loc.coordinates.lng) {
            const coordsElement = document.createElement('div');
            const coordsText = `${loc.coordinates.lat.toFixed(6)}, ${loc.coordinates.lng.toFixed(6)}`;
            coordsElement.innerHTML = `
            <div style="
                display: flex; 
                align-items: center; 
                justify-content: space-between;
                padding: 10px 12px;
                background: rgba(102, 126, 234, 0.08);
                border-radius: 8px;
                margin-bottom: 16px;
                border: 1px solid rgba(102, 126, 234, 0.15);
            ">
                <span style="font-size: 13px; color: #667eea; font-family: monospace;">
                    🌍 ${coordsText}
                </span>
                <button onclick="navigator.clipboard.writeText('${coordsText}')" style="
                    background: none;
                    border: none;
                    color: #667eea;
                    cursor: pointer;
                    padding: 4px;
                    border-radius: 4px;
                    transition: all 0.2s ease;
                " onmouseenter="this.style.background='rgba(102, 126, 234, 0.1)'" onmouseleave="this.style.background='none'" title="Copy coordinates">
                    📋
                </button>
            </div>
        `;
            contentWrapper.appendChild(coordsElement);
        }

        // Enhanced actions
        const actionsDiv = document.createElement('div');
        Object.assign(actionsDiv.style, {
            display: 'flex',
            gap: '12px',
            alignItems: 'center',
            justifyContent: 'flex-end'
        });

        // Edit button with gradient
        const editLink = document.createElement('a');
        const id = loc.id || loc.Id || loc._id || '';
        editLink.href = id ? `${this.editUrlBase}/${id}` : '#';
        if (!id) {
            editLink.style.pointerEvents = 'none';
            editLink.style.opacity = '0.5';
        }
        editLink.innerHTML = '✏️ Chỉnh sửa';
        Object.assign(editLink.style, {
            background: 'linear-gradient(135deg, #28a745, #20c997)',
            color: 'white',
            textDecoration: 'none',
            fontSize: '13px',
            fontWeight: '500',
            padding: '10px 16px',
            borderRadius: '8px',
            transition: 'all 0.2s ease',
            boxShadow: '0 4px 12px rgba(40, 167, 69, 0.3)',
            display: 'flex',
            alignItems: 'center',
            gap: '6px'
        });
        editLink.addEventListener('mouseenter', () => {
            editLink.style.transform = 'translateY(-2px)';
            editLink.style.boxShadow = '0 6px 16px rgba(40, 167, 69, 0.4)';
        });
        editLink.addEventListener('mouseleave', () => {
            editLink.style.transform = 'translateY(0)';
            editLink.style.boxShadow = '0 4px 12px rgba(40, 167, 69, 0.3)';
        });
        actionsDiv.appendChild(editLink);

        // Delete button with gradient
        const deleteButton = document.createElement('button');
        deleteButton.innerHTML = '🗑️ Xóa';
        Object.assign(deleteButton.style, {
            background: 'linear-gradient(135deg, #dc3545, #c82333)',
            color: 'white',
            border: 'none',
            cursor: 'pointer',
            fontSize: '13px',
            fontWeight: '500',
            padding: '10px 16px',
            borderRadius: '8px',
            transition: 'all 0.2s ease',
            boxShadow: '0 4px 12px rgba(220, 53, 69, 0.3)',
            display: 'flex',
            alignItems: 'center',
            gap: '6px'
        });
        deleteButton.addEventListener('mouseenter', () => {
            deleteButton.style.transform = 'translateY(-2px)';
            deleteButton.style.boxShadow = '0 6px 16px rgba(220, 53, 69, 0.4)';
            deleteButton.style.animation = 'pulse 0.5s ease-in-out';
        });
        deleteButton.addEventListener('mouseleave', () => {
            deleteButton.style.transform = 'translateY(0)';
            deleteButton.style.boxShadow = '0 4px 12px rgba(220, 53, 69, 0.3)';
            deleteButton.style.animation = 'none';
        });

        deleteButton.addEventListener('click', () => this.handleDelete(loc.id, loc.name));
        actionsDiv.appendChild(deleteButton);

        contentWrapper.appendChild(actionsDiv);
        listItem.appendChild(contentWrapper);
        return listItem;
    }


    handleSearch(query) {
        const lowerQuery = query.toLowerCase().trim();

        if (!lowerQuery) {
            this.filteredLocations = [...this.locations];
        } else {
            this.filteredLocations = this.locations.filter(loc => {
                const nameMatch = loc.name && loc.name.toLowerCase().includes(lowerQuery);
                const descMatch = loc.description && loc.description.toLowerCase().includes(lowerQuery);
                return nameMatch || descMatch;
            });
        }

        this.renderList();
    }

    async handleDelete(locationId, locationName) {
        const confirmMessage = `Bạn có chắc chắn muốn xóa địa điểm "${locationName || 'này'}"?\n\nHành động này không thể hoàn tác.`;

        if (!window.confirm(confirmMessage)) return;

        try {
            const response = await fetch(`/api/locations/${locationId}`, {
                method: "DELETE",
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.error || `HTTP ${response.status}: ${response.statusText}`);
            }

            this.showSuccessMessage(data.message || "Địa điểm đã được xóa thành công!");

            this.locations = this.locations.filter(loc => loc._id !== locationId);
            this.filteredLocations = this.filteredLocations.filter(loc => loc._id !== locationId);

            this.renderList();

            if (this.onLocationDeletedCallback) {
                this.onLocationDeletedCallback(locationId);
            }

        } catch (err) {
            console.error("Lỗi khi xóa địa điểm:", err);
            this.showErrorMessage("Có lỗi khi xóa địa điểm: " + err.message);
        }
    }

    showSuccessMessage(message) {
        const messageDiv = document.createElement('div');
        Object.assign(messageDiv.style, {
            position: 'fixed',
            top: '30px',
            right: '30px',
            padding: '16px 24px',
            background: 'linear-gradient(135deg, #d4edda, #c3e6cb)',
            color: '#155724',
            border: '1px solid #c3e6cb',
            borderRadius: '12px',
            boxShadow: '0 8px 32px rgba(0, 0, 0, 0.15)',
            zIndex: '10001',
            maxWidth: '350px',
            fontSize: '14px',
            fontWeight: '500',
            backdropFilter: 'blur(10px)',
            animation: 'slideInRight 0.3s ease-out'
        });
        messageDiv.innerHTML = `<span style="margin-right: 8px;">✅</span>${message}`;

        document.body.appendChild(messageDiv);

        setTimeout(() => {
            messageDiv.style.animation = 'slideInRight 0.3s ease-out reverse';
            setTimeout(() => {
                if (messageDiv.parentNode) {
                    messageDiv.parentNode.removeChild(messageDiv);
                }
            }, 300);
        }, 3000);
    }

    // Error message
    showErrorMessage(message) {
        const messageDiv = document.createElement('div');
        Object.assign(messageDiv.style, {
            position: 'fixed',
            top: '20px',
            right: '20px',
            padding: '12px 20px',
            backgroundColor: '#f8d7da',
            color: '#721c24',
            border: '1px solid #f5c6cb',
            borderRadius: '8px',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
            zIndex: '10000',
            maxWidth: '300px',
            fontSize: '14px'
        });
        messageDiv.textContent = message;

        document.body.appendChild(messageDiv);

        setTimeout(() => {
            if (messageDiv.parentNode) {
                messageDiv.parentNode.removeChild(messageDiv);
            }
        }, 5000);
    }

    // Public method to get current state
    getCurrentState() {
        return {
            isVisible: this.isVisible,
            currentProvinceGid: this.currentProvinceGid,
            currentProvinceName: this.currentProvinceName,
            locationsCount: this.locations.length,
            filteredLocationsCount: this.filteredLocations.length,
            locationsVersion: this.locationsVersion
        };
    }
}