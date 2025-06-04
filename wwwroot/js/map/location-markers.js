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
                    marker.bindPopup(`<b></b>`);
                    marker.addTo(this.map);
                    this.markers.push(marker);
                });
            });
    }


    clearMarkers() {
        this.markers.forEach(m => this.map.removeLayer(m));
        this.markers = [];
    }
}
