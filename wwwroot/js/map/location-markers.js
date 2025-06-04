class LocationMarkers {
    constructor(map) {
        this.map = map;
        this.markers = [];
    }

    loadMarkers(provinceGid) {
        fetch(`/api/locations?gid=${provinceGid}`)
            .then(res => res.json())
            .then(json => {
                this.clearMarkers();
                json.data.forEach(location => {
                    const pulseIcon = L.icon.pulse({
                        iconSize: [20, 20],
                        color: "black",
                        heartbeat: 1,
                    });

                    const marker = L.marker([location.coordinates.lat, location.coordinates.lng], { icon: pulseIcon });

                    // Tạo popup với nút bấm
                    const popupContent = `
                        <div style="text-align: center; min-width: 150px;">
                            <h4 style="margin: 5px 0; color: #333;">${location.name}</h4>
                            <button 
                                onclick="window.open('https://localhost:7128/attractions/${location.id}', '_blank')"
                                style="
                                    background: #007bff;
                                    color: white;
                                    border: none;
                                    padding: 8px 16px;
                                    border-radius: 4px;
                                    cursor: pointer;
                                    font-size: 14px;
                                    margin-top: 5px;
                                    transition: background 0.3s;
                                "
                                onmouseover="this.style.background='#0056b3'"
                                onmouseout="this.style.background='#007bff'"
                            >
                                Xem chi tiết
                            </button>
                        </div>
                    `;

                    marker.bindPopup(popupContent);

                    // Sự kiện click marker chỉ mở popup
                    marker.on('click', () => {
                        marker.openPopup();
                        console.log('Clicked on attraction:', location.name);
                    });

                    marker.addTo(this.map);
                    this.markers.push(marker);
                });
            })
            .catch(error => {
                console.error('Error loading markers:', error);
            });
    }

    clearMarkers() {
        this.markers.forEach(m => this.map.removeLayer(m));
        this.markers = [];
    }
}