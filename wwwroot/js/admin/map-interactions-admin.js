document.addEventListener('DOMContentLoaded', function () {
    // --- Biến quản lý ---
    var map = null;
    var geojsonLayer = null;
    var clickedLayer = null;
    var clickedLayerGid = null;
    var originalStyles = {};
    var searchQuery = '';
    var suggestions = [];
    var locationMarkers = null;
    var locationList = null;
    var isMounted = false;


    var MAP_CONFIG = {
        tileUrl: 'https://api.maptiler.com/maps/streets-v2/{z}/{x}/{y}.png?key=RCfpOibQtfADVJ8TBhgS',
        attribution: '&copy; <a href="https://www.maptiler.com/copyright/">MapTiler</a> contributors',
        center: [14.0583, 108.2772],
        zoom: 5,
        selectedZoom: 8
    };

    var PROVINCE_STYLE_DEFAULTS = {
        color: "#ff7800",
        weight: 2,
        opacity: 0.6,
        fillColor: "#ff0000",
        fillOpacity: 0.4
    };
    var SELECTED_STYLE = { fillColor: "#ff0000", color: "#ff7800" };
    var DESELECTED_STYLE = { fillColor: "#ffffff", color: "#ffffff" };

    // DOM elements
    var searchInput = document.getElementById('search-input');
    var suggestionsList = document.getElementById('suggestions-list');
    var backButton = document.getElementById('back-button');

    // --- Các hàm chính ---
    function lockMapInteraction(mapObj) {
        if (!mapObj) return;
        mapObj.dragging.disable();
        mapObj.scrollWheelZoom.disable();
        mapObj.doubleClickZoom.disable();
        mapObj.boxZoom.disable();
        mapObj.keyboard.disable();
        mapObj.touchZoom.disable();
    }
    function unlockMapInteraction(mapObj) {
        if (!mapObj) return;
        mapObj.dragging.enable();
        mapObj.scrollWheelZoom.enable();
        mapObj.doubleClickZoom.enable();
        mapObj.boxZoom.enable();
        mapObj.keyboard.enable();
        mapObj.touchZoom.enable();
    }

    // Khi click tỉnh
    // Fixed handleProvinceClick function - remove the premature state setting
    function handleProvinceClick(layer, feature, mapObj, gjLayer) {
        try {
            if (!layer.getBounds || !feature || !feature.properties) {
                console.error('Invalid layer or feature data');
                return;
            }
            var bounds = layer.getBounds();
            var center = bounds.getCenter();

            clickedLayer = feature.properties.ten_tinh;
            clickedLayerGid = feature.properties.gid;
            mapObj.setView(center, MAP_CONFIG.selectedZoom);

            lockMapInteraction(mapObj);

            gjLayer.eachLayer(function (l) {
                if (l !== layer) {
                    l.off("click");
                    l.setStyle(DESELECTED_STYLE);
                }
            });
            layer.setStyle(SELECTED_STYLE);

            updateBackButtonVisibility(true);

            // REMOVED: Don't set state to 2 here
            // The loadMarkers function will set it to 1 for new provinces
            // and the zoom event will handle transition from 1 to ready state

            updateLocationMarkers({ provinceGid: clickedLayerGid, shouldClear: false });

            layer.openPopup();

            if (locationList) {
                locationList.show(feature.properties.gid, feature.properties.ten_tinh, Date.now());
            }
        } catch (error) {
            console.error('Error in handleProvinceClick:', error);
        }
    }

    // Quản lý marker
    function updateLocationMarkers(props) {
        if (!locationMarkers) return;

        var provinceGid = props.provinceGid;
        var shouldClear = props.shouldClear;

        if (shouldClear) {
            locationMarkers.clearMarkers();
            return;
        }

        if (provinceGid) {
            console.log('Loading markers for province gid:', provinceGid);
            locationMarkers.loadMarkers(provinceGid);
        }
    }

    // Tải dữ liệu tỉnh (geojson)
    function loadProvinceData() {
        fetch('/data/province.json')
            .then(function (response) {
                if (!response.ok) throw new Error('Failed to load province data: ' + response.status);
                return response.json();
            })
            .then(function (data) {
                if (!data || !data.features) throw new Error('Invalid province data format');

                geojsonLayer = L.geoJSON(data, {
                    style: PROVINCE_STYLE_DEFAULTS,
                    onEachFeature: function (feature, layer) {
                        if (!feature.properties) return;

                        originalStyles[feature.properties.ten_tinh] = Object.assign({}, PROVINCE_STYLE_DEFAULTS);

                        layer.on('click', function () {
                            handleProvinceClick(layer, feature, map, geojsonLayer);
                        });
                    }
                }).addTo(map);

                // Sau khi load geojson, set geojsonLayer cho locationMarkers
                if (locationMarkers) {
                    locationMarkers.setGeoJsonLayer(geojsonLayer);
                }

                initializeUI();
                console.log('Province data loaded successfully');
            })
            .catch(function (error) {
                console.error('Error loading province data:', error);
                alert('Không thể tải dữ liệu bản đồ. Vui lòng thử lại sau.');
            });
    }

    // Khởi tạo UI (style input, button, event)
    function initializeUI() {
        styleSearchInput();
        styleBackButton();
        attachEventListeners();
        console.log('UI initialized successfully');
    }

    // Hiển thị/ẩn nút back
    function updateBackButtonVisibility(show) {
        if (!backButton) return;
        backButton.style.display = show ? 'block' : 'none';
        if (show) {
            backButton.style.opacity = '0';
            backButton.style.transform = 'translateY(-10px)';
            setTimeout(function () {
                backButton.style.transition = 'all 0.3s ease';
                backButton.style.opacity = '1';
                backButton.style.transform = 'translateY(0)';
            }, 10);
        }
    }

    // Nút back click
    function handleBackClick() {
        if (!map || !geojsonLayer) return;
        try {
            map.setView(MAP_CONFIG.center, MAP_CONFIG.zoom);
            unlockMapInteraction(map);

            geojsonLayer.eachLayer(function (layer) {
                if (!layer.feature) return;
                var featureName = layer.feature.properties.ten_tinh;
                var originalStyle = originalStyles[featureName] || PROVINCE_STYLE_DEFAULTS;

                layer.setStyle({
                    color: originalStyle.color,
                    fillColor: originalStyle.fillColor,
                    fillOpacity: originalStyle.fillOpacity,
                    weight: originalStyle.weight || PROVINCE_STYLE_DEFAULTS.weight,
                    opacity: originalStyle.opacity || PROVINCE_STYLE_DEFAULTS.opacity
                });

                layer.off('click');
                layer.on('click', function () {
                    handleProvinceClick(layer, layer.feature, map, geojsonLayer);
                });

                layer.closePopup();
            });

            clickedLayer = null;
            clickedLayerGid = null;

            // Reset về trạng thái 0 khi back về trang chủ
            if (locationMarkers) {
                locationMarkers.setProvinceInteractionState(0);
            }

            updateBackButtonVisibility(false);

            if (locationList) locationList.hide();

            updateLocationMarkers({ provinceGid: null, shouldClear: true });

        } catch (error) {
            console.error('Error in handleBackClick:', error);
        }
    }


    // Tạo style input tìm kiếm
    function styleSearchInput() {
        if (!searchInput) return;
        Object.assign(searchInput.style, {
            position: 'absolute',
            top: '60px',
            left: '20px',
            zIndex: '1000',
            padding: '12px 20px',
            fontSize: '16px',
            borderRadius: '8px',
            border: '1px solid #ddd',
            backgroundColor: '#f9f9f9',
            color: '#333',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.1)',
            transition: 'all 0.3s ease',
            width: '250px',
            outline: 'none'
        });
    }

    // Style back button
    function styleBackButton() {
        if (!backButton) return;
        Object.assign(backButton.style, {
            position: 'absolute',
            top: '20px',
            left: '20px',
            zIndex: '1000',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            padding: '12px 24px',
            borderRadius: '8px',
            cursor: 'pointer',
            fontSize: '18px',
            boxShadow: '0 6px 14px rgba(0, 123, 255, 0.6)',
            display: 'none',
            transition: 'all 0.3s ease',
            outline: 'none'
        });
        backButton.addEventListener('mouseenter', function () {
            this.style.backgroundColor = '#0056b3';
            this.style.transform = 'translateY(-2px)';
        });
        backButton.addEventListener('mouseleave', function () {
            this.style.backgroundColor = '#007bff';
            this.style.transform = 'translateY(0)';
        });
    }

    // Gắn sự kiện cho input, nút, document click
    function attachEventListeners() {
        if (searchInput) {
            searchInput.addEventListener('input', handleSearchChange);
            searchInput.addEventListener('focus', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 112, 255, 0.2)';
                e.target.style.backgroundColor = '#ffffff';
            });
            searchInput.addEventListener('blur', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
                e.target.style.backgroundColor = '#f9f9f9';
                setTimeout(hideSuggestions, 200);
            });
            searchInput.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') {
                    hideSuggestions();
                    searchInput.blur();
                }
            });
        }
        if (backButton) {
            backButton.addEventListener('click', handleBackClick);
        }
        document.addEventListener('click', function (e) {
            if (searchInput && suggestionsList &&
                !searchInput.contains(e.target) &&
                !suggestionsList.contains(e.target)) {
                hideSuggestions();
            }
        });
    }

    // Hàm lọc suggestions (tỉnh)
    function filterSuggestions(query) {
        if (!geojsonLayer || !query || query.trim() === '') return [];
        var filtered = [];
        var q = query.toLowerCase().trim();
        geojsonLayer.eachLayer(function (layer) {
            if (layer.feature && layer.feature.properties.ten_tinh) {
                var name = layer.feature.properties.ten_tinh;
                if (name.toLowerCase().includes(q) && filtered.indexOf(name) === -1) {
                    filtered.push(name);
                }
            }
        });
        return filtered.sort();
    }

    // Hiển thị danh sách gợi ý tìm kiếm
    function displaySuggestions(suggestionList) {
        if (!suggestionList || suggestionList.length === 0) {
            hideSuggestions();
            return;
        }
        suggestionsList.innerHTML = '';
        suggestionsList.style.display = 'block';
        Object.assign(suggestionsList.style, {
            position: 'absolute',
            top: '130px',
            left: '20px',
            zIndex: '1000',
            backgroundColor: 'white',
            width: 'calc(100% - 40px)',
            maxWidth: '300px',
            border: '1px solid #ccc',
            borderRadius: '8px',
            maxHeight: '200px',
            overflowY: 'auto',
            boxShadow: '0 4px 10px rgba(0, 0, 0, 0.1)',
            listStyle: 'none',
            padding: '0',
            margin: '0'
        });
        suggestionList.forEach(function (suggestion, i) {
            var li = document.createElement('li');
            li.textContent = suggestion;
            Object.assign(li.style, {
                padding: '8px 16px',
                cursor: 'pointer',
                backgroundColor: '#f7f7f7',
                borderBottom: i < suggestionList.length - 1 ? '1px solid #ddd' : 'none',
                transition: 'background-color 0.2s ease'
            });
            li.addEventListener('mouseenter', function () {
                this.style.backgroundColor = '#e9ecef';
            });
            li.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '#f7f7f7';
            });
            li.addEventListener('mousedown', function (e) {
                e.preventDefault();
                handleProvinceSuggestionClick(suggestion);
            });
            suggestionsList.appendChild(li);
        });
    }

    function hideSuggestions() {
        if (suggestionsList) suggestionsList.style.display = 'none';
    }

    // Xử lý click tỉnh trong list gợi ý
    function handleProvinceSuggestionClick(provinceName) {
        if (!geojsonLayer || !map) {
            console.error('Map or GeoJSON layer not available');
            return;
        }
        var targetLayer = null;
        geojsonLayer.eachLayer(function (layer) {
            if (layer.feature && layer.feature.properties.ten_tinh === provinceName) {
                targetLayer = layer;
            }
        });
        if (targetLayer) {
            handleProvinceClick(targetLayer, targetLayer.feature, map, geojsonLayer);
            searchInput.value = '';
            searchQuery = '';
        } else {
            console.error('Province not found:', provinceName);
        }
        hideSuggestions();
    }

    // Debounce xử lý tìm kiếm
    var searchTimeout;
    function handleSearchChange(e) {
        var q = e.target.value;
        searchQuery = q;
        if (searchTimeout) clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            if (q && q.trim() !== '') {
                var sugg = filterSuggestions(q);
                displaySuggestions(sugg);
            } else {
                hideSuggestions();
            }
        }, 150);
    }

    // Khởi tạo map và các component chính
    function initializeMap() {
        if (isMounted) return;

        try {
            map = L.map('map').setView(MAP_CONFIG.center, MAP_CONFIG.zoom);

            L.tileLayer(MAP_CONFIG.tileUrl, {
                attribution: MAP_CONFIG.attribution,
                tileSize: 256,
                zoomOffset: 0
            }).addTo(map);

            // Khởi tạo LocationList nếu có
            if (typeof LocationList !== 'undefined') {
                locationList = new LocationList({
                    containerId: 'location-list-container',
                    onLocationDeletedCallback: function (locationId) {
                        if (locationMarkers) {
                            locationMarkers.removeMarkerByLocationId(locationId);
                        }
                    },
                    onLocationAddedCallback: null,
                    editUrlBase: '/admin/locations/edit/',
                    provinceDetailUrlBase: '/provinces/'
                });
                console.log('LocationList initialized successfully');
            } else {
                console.warn('LocationList class not found.');
            }

            // Khởi tạo LocationMarkers, ban đầu geojsonLayer là null
            if (typeof LocationMarkers !== 'undefined') {
                locationMarkers = new LocationMarkers(map, null);
                // Gắn locationList vào locationMarkers (nhớ method setLocationList phải có)
                if (locationList) {
                    locationMarkers.setLocationList(locationList);
                }
                console.log('LocationMarkers initialized successfully');
            } else {
                console.warn('LocationMarkers class not found. Some features may not work.');
            }

            // Load dữ liệu tỉnh
            loadProvinceData();

            isMounted = true;
        } catch (error) {
            console.error('Error initializing map:', error);
        }
    }

    // Khởi tạo UI style và event
    function initializeUI() {
        styleSearchInput();
        styleBackButton();
        attachEventListeners();
        console.log('UI initialized successfully');
    }

    // Gắn event listener cho các element
    function attachEventListeners() {
        if (searchInput) {
            searchInput.addEventListener('input', handleSearchChange);
            searchInput.addEventListener('focus', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 112, 255, 0.2)';
                e.target.style.backgroundColor = '#ffffff';
            });
            searchInput.addEventListener('blur', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
                e.target.style.backgroundColor = '#f9f9f9';
                setTimeout(hideSuggestions, 200);
            });
            searchInput.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') {
                    hideSuggestions();
                    searchInput.blur();
                }
            });
        }
        if (backButton) {
            backButton.addEventListener('click', handleBackClick);
        }
        document.addEventListener('click', function (e) {
            if (searchInput && suggestionsList &&
                !searchInput.contains(e.target) &&
                !suggestionsList.contains(e.target)) {
                hideSuggestions();
            }
        });
    }

    // Debug functions xuất ra window
    function setupDebugging() {
        window.getMapInstance = function () { return map; };
        window.getClickedLayerGid = function () { return clickedLayerGid; };
        window.isProvinceSelected = function () { return clickedLayer !== null; };
        window.getLocationMarkers = function () { return locationMarkers; };
        window.getCurrentState = function () {
            return {
                isMounted: isMounted,
                clickedLayer: clickedLayer,
                clickedLayerGid: clickedLayerGid,
                searchQuery: searchQuery,
                suggestionsCount: suggestions.length
            };
        };
        console.log('Debug functions available: getMapInstance, getClickedLayerGid, isProvinceSelected, getLocationMarkers, getCurrentState');
    }

    // Khởi chạy sau 100ms
    setTimeout(function () {
        initializeMap();
        setupDebugging();
    }, 100);
});
