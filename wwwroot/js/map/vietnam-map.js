document.addEventListener('DOMContentLoaded', function () {
    // Enhanced Variables with better state management
    var map = null;
    var geojsonLayer = null;
    var clickedLayer = null;
    var clickedLayerGid = null;
    var originalStyles = {};
    var searchQuery = '';
    var suggestions = [];
    var locationMarkers = null;
    var isMounted = false; // Add mounting state like Next.js
    
    // Enhanced API configuration (matching Next.js)
    var MAP_CONFIG = {
        tileUrl: 'https://api.maptiler.com/maps/streets-v2/{z}/{x}/{y}.png?key=RCfpOibQtfADVJ8TBhgS',
        attribution: '&copy; <a href="https://www.maptiler.com/copyright/">MapTiler</a> contributors',
        center: [14.0583, 108.2772],
        zoom: 5,
        selectedZoom: 8
    };

    // Enhanced Type-like definitions (JavaScript objects for validation)
    var PROVINCE_STYLE_DEFAULTS = {
        color: "#ff7800",
        weight: 2,
        opacity: 0.6,
        fillColor: "#ff0000",
        fillOpacity: 0.4
    };

    var SELECTED_STYLE = {
        fillColor: "#ff0000",
        color: "#ff7800"
    };

    var DESELECTED_STYLE = {
        fillColor: "#ffffff",
        color: "#ffffff"
    };

    // DOM Elements
    var searchInput = document.getElementById('search-input');
    var suggestionsList = document.getElementById('suggestions-list');
    var backButton = document.getElementById('back-button');

    // Enhanced Map interaction functions
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

    // Enhanced Province click handler with better error handling
    function handleProvinceClick(layer, feature, mapObj, gjLayer) {
        try {
            if (!layer.getBounds || !feature || !feature.properties) {
                console.error('Invalid layer or feature data');
                return;
            }

            var bounds = layer.getBounds();
            var center = bounds.getCenter();

            // Update state
            clickedLayer = feature.properties.ten_tinh;
            if (feature.properties.gid !== undefined) {
                clickedLayerGid = feature.properties.gid;
            }

            // Set map view
            mapObj.setView(center, MAP_CONFIG.selectedZoom);
            lockMapInteraction(mapObj);

            // Style other layers
            gjLayer.eachLayer(function (l) {
                if (l !== layer) {
                    l.off("click");
                    l.setStyle(DESELECTED_STYLE);
                }
            });

            // Style clicked layer
            layer.setStyle(SELECTED_STYLE);

            // Show back button with enhanced styling
            updateBackButtonVisibility(true);

            // Enhanced LocationMarkers handling (like Next.js props)
            updateLocationMarkers({
                provinceGid: feature.properties.gid,
                shouldClear: false
            });

            layer.openPopup();
        } catch (error) {
            console.error('Error in handleProvinceClick:', error);
        }
    }

    // Enhanced LocationMarkers management (simulating Next.js props)
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

    // Enhanced search functionality with better filtering
    function filterSuggestions(query) {
        if (!geojsonLayer || !query || query.trim() === '') return [];

        var filteredSuggestions = [];
        var queryLower = query.toLowerCase().trim();

        geojsonLayer.eachLayer(function (layer) {
            if (layer.feature && layer.feature.properties.ten_tinh) {
                var provinceName = layer.feature.properties.ten_tinh;
                if (provinceName.toLowerCase().includes(queryLower)) {
                    // Avoid duplicates
                    if (filteredSuggestions.indexOf(provinceName) === -1) {
                        filteredSuggestions.push(provinceName);
                    }
                }
            }
        });

        // Sort suggestions for better UX
        return filteredSuggestions.sort();
    }

    // Enhanced search change handler with debouncing
    var searchTimeout;
    function handleSearchChange(e) {
        var query = e.target.value;
        searchQuery = query;

        // Clear previous timeout
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }

        // Debounce search for better performance
        searchTimeout = setTimeout(function() {
            if (query && query.trim() !== '') {
                suggestions = filterSuggestions(query);
                displaySuggestions(suggestions);
            } else {
                suggestions = [];
                hideSuggestions();
            }
        }, 150); // 150ms debounce like modern search
    }

    // Enhanced suggestion display with better styling
    function displaySuggestions(suggestionList) {
        if (!suggestionList || suggestionList.length === 0) {
            hideSuggestions();
            return;
        }

        suggestionsList.innerHTML = '';
        suggestionsList.style.display = 'block';

        // Enhanced styling (matching Next.js exactly)
        Object.assign(suggestionsList.style, {
            position: 'absolute',
            top: '130px',
            left: '20px',
            zIndex: '1000',
            backgroundColor: 'white',
            width: 'calc(100% - 40px)',
            maxWidth: '300px', // Add max width for better design
            border: '1px solid #ccc',
            borderRadius: '8px',
            maxHeight: '200px',
            overflowY: 'auto',
            boxShadow: '0 4px 10px rgba(0, 0, 0, 0.1)',
            listStyle: 'none',
            padding: '0',
            margin: '0'
        });

        suggestionList.forEach(function (suggestion, index) {
            var li = document.createElement('li');
            li.textContent = suggestion;
            
            // Enhanced styling
            Object.assign(li.style, {
                padding: '8px 16px',
                cursor: 'pointer',
                backgroundColor: '#f7f7f7',
                borderBottom: index < suggestionList.length - 1 ? '1px solid #ddd' : 'none',
                transition: 'background-color 0.2s ease' // Smooth transition
            });

            // Enhanced hover effects
            li.addEventListener('mouseenter', function () {
                this.style.backgroundColor = '#e9ecef';
            });

            li.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '#f7f7f7';
            });

            // Use mousedown to prevent blur event interference
            li.addEventListener('mousedown', function (e) {
                e.preventDefault(); // Prevent input blur
                handleProvinceSuggestionClick(suggestion);
            });

            suggestionsList.appendChild(li);
        });
    }

    function hideSuggestions() {
        if (suggestionsList) {
            suggestionsList.style.display = 'none';
        }
    }

    // Enhanced province suggestion click handler
    function handleProvinceSuggestionClick(provinceName) {
        console.log("Selecting province:", provinceName);

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
            
            // Clear search input (like Next.js)
            searchInput.value = '';
            searchQuery = '';
        } else {
            console.error('Province not found:', provinceName);
        }

        hideSuggestions();
    }

    // Enhanced back button functionality
    function updateBackButtonVisibility(show) {
        if (!backButton) return;
        
        backButton.style.display = show ? 'block' : 'none';
        
        // Add smooth transition effect
        if (show) {
            backButton.style.opacity = '0';
            backButton.style.transform = 'translateY(-10px)';
            setTimeout(function() {
                backButton.style.transition = 'all 0.3s ease';
                backButton.style.opacity = '1';
                backButton.style.transform = 'translateY(0)';
            }, 10);
        }
    }

    // Enhanced back click handler
    function handleBackClick() {
        if (!map || !geojsonLayer) return;

        try {
            // Reset map view
            map.setView(MAP_CONFIG.center, MAP_CONFIG.zoom);
            unlockMapInteraction(map);

            // Reset all layers
            geojsonLayer.eachLayer(function (layer) {
                if (!layer.feature) return;

                var featureName = layer.feature.properties.ten_tinh;
                var originalStyle = originalStyles[featureName] || PROVINCE_STYLE_DEFAULTS;

                // Reset style
                layer.setStyle({
                    color: originalStyle.color,
                    fillColor: originalStyle.fillColor,
                    fillOpacity: originalStyle.fillOpacity,
                    weight: originalStyle.weight || PROVINCE_STYLE_DEFAULTS.weight,
                    opacity: originalStyle.opacity || PROVINCE_STYLE_DEFAULTS.opacity
                });

                // Recreate popup
                var popupContent = createPopupContent(layer.feature);
                layer.bindPopup(popupContent);

                // Re-attach click event
                layer.off('click');
                layer.on("click", function () {
                    handleProvinceClick(layer, layer.feature, map, geojsonLayer);
                });

                layer.closePopup();
            });

            // Reset state (like Next.js)
            clickedLayer = null;
            clickedLayerGid = null;
            
            // Hide back button
            updateBackButtonVisibility(false);

            // Clear location markers (simulating Next.js shouldClear prop)
            updateLocationMarkers({
                provinceGid: null,
                shouldClear: true
            });

        } catch (error) {
            console.error('Error in handleBackClick:', error);
        }
    }

    // Enhanced popup content creation
    function createPopupContent(feature) {
        if (!feature || !feature.properties) return '';

        const url = 'https://localhost:7128/province/details?provinceGid=' + (feature.properties.gid || '');

        return '<div style="font-family: Arial, sans-serif; padding: 8px; max-width: 250px;">' +
            '<h3 style="margin: 0 0 8px 0; font-size: 20px; font-weight: bold; color: #333;">' +
            (feature.properties.ten_tinh || 'Unknown Province') + '</h3>' +
            (feature.properties.gid
                ? '<p style="margin: 4px 0 12px 0; font-size: 14px; color: #555;"><strong>Mã tỉnh:</strong> ' + feature.properties.gid + '</p>'
                : ""
            ) +
            '<div style="text-align: center;">' +
            // Button thay cho a href, dùng onclick mở tab mới
            '<button onclick="window.open(\'' + url + '\', \'_blank\')" ' +
            'style="cursor: pointer; padding: 8px 16px; background-color: #28a745; color: white; ' +
            'border: none; border-radius: 8px; font-weight: bold; font-size: 14px; ' +
            'transition: background-color 0.2s ease;" ' +
            'onmouseover="this.style.backgroundColor=\'#218838\'" ' +
            'onmouseout="this.style.backgroundColor=\'#28a745\'">' +
            'Xem thông tin và tham quan' +
            '</button>' +
            '</div>' +
            '</div>';
    }



    // Enhanced search input styling (matching Next.js exactly)
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
            outline: 'none' // Remove default outline
        });
    }

    // Enhanced back button styling
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

        // Add hover effect
        backButton.addEventListener('mouseenter', function() {
            this.style.backgroundColor = '#0056b3';
            this.style.transform = 'translateY(-2px)';
        });

        backButton.addEventListener('mouseleave', function() {
            this.style.backgroundColor = '#007bff';
            this.style.transform = 'translateY(0)';
        });
    }

    // Enhanced event listeners
    function attachEventListeners() {
        // Enhanced search input events
        if (searchInput) {
            searchInput.addEventListener('input', handleSearchChange);

            // Dynamic focus/blur effects (like Next.js)
            searchInput.addEventListener('focus', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 112, 255, 0.2)';
                e.target.style.backgroundColor = '#ffffff';
            });

            searchInput.addEventListener('blur', function (e) {
                e.target.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
                e.target.style.backgroundColor = '#f9f9f9';
                
                // Delay hiding suggestions to allow clicking
                setTimeout(hideSuggestions, 200);
            });

            // Handle keyboard navigation
            searchInput.addEventListener('keydown', function(e) {
                if (e.key === 'Escape') {
                    hideSuggestions();
                    searchInput.blur();
                }
            });
        }

        // Back button event
        if (backButton) {
            backButton.addEventListener('click', handleBackClick);
        }

        // Enhanced click outside handler
        document.addEventListener('click', function (e) {
            if (searchInput && suggestionsList &&
                !searchInput.contains(e.target) &&
                !suggestionsList.contains(e.target)) {
                hideSuggestions();
            }
        });
    }

    // Enhanced initialization with mounting state (like Next.js useEffect)
    function initializeMap() {
        if (isMounted) return; // Prevent double initialization
        
        try {
            // Initialize map
            map = L.map('map').setView(MAP_CONFIG.center, MAP_CONFIG.zoom);

            // Add tile layer with enhanced config
            L.tileLayer(MAP_CONFIG.tileUrl, {
                attribution: MAP_CONFIG.attribution,
                tileSize: 256,
                zoomOffset: 0,
            }).addTo(map);

            // Initialize LocationMarkers
            if (typeof LocationMarkers !== 'undefined') {
                locationMarkers = new LocationMarkers(map);
                console.log('LocationMarkers initialized successfully');
            } else {
                console.warn('LocationMarkers class not found. Some features may not work.');
            }

            // Load province data
            loadProvinceData();
            
            isMounted = true;
            
        } catch (error) {
            console.error('Error initializing map:', error);
        }
    }

    // Enhanced province data loading
    function loadProvinceData() {
        fetch('/data/province.json')
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Failed to load province data: ' + response.status);
                }
                return response.json();
            })
            .then(function (data) {
                if (!data || !data.features) {
                    throw new Error('Invalid province data format');
                }

                geojsonLayer = L.geoJSON(data, {
                    style: function (feature) {
                        return PROVINCE_STYLE_DEFAULTS;
                    },
                    onEachFeature: function (feature, layer) {
                        if (!feature.properties) return;

                        // Store enhanced original styles
                        originalStyles[feature.properties.ten_tinh] = Object.assign({}, PROVINCE_STYLE_DEFAULTS);

                        // Create popup
                        var popupContent = createPopupContent(feature);
                        layer.bindPopup(popupContent);

                        // Attach click event
                        layer.on('click', function () {
                            handleProvinceClick(layer, feature, map, geojsonLayer);
                        });
                    }
                }).addTo(map);

                // Initialize UI after data loads
                initializeUI();
                
                console.log('Province data loaded successfully');
            })
            .catch(function (error) {
                console.error('Error loading province data:', error);
                // Show user-friendly error message
                alert('Không thể tải dữ liệu bản đồ. Vui lòng thử lại sau.');
            });
    }

    // Enhanced UI initialization
    function initializeUI() {
        styleSearchInput();
        styleBackButton();
        attachEventListeners();
        
        console.log('UI initialized successfully');
    }

    // Enhanced error handling and debugging
    function setupDebugging() {
        // Export enhanced debugging functions
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

    // Initialize everything with timeout (like Next.js)
    setTimeout(function() {
        initializeMap();
        setupDebugging();
    }, 100);
});