// MapLibre GL JS funciones
let maps = {};
let markers = {};

window.initializeMap = (mapId, lng, lat, zoom, dotNetHelper) => {
    if (maps[mapId]) {
        maps[mapId].remove();
    }

    maps[mapId] = new maplibregl.Map({
        container: mapId,
        style: {
            version: 8,
            sources: {
                'osm-tiles': {
                    type: 'raster',
                    tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
                    tileSize: 256,
                    attribution: '© <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                }
            },
            layers: [{
                id: 'osm-layer',
                type: 'raster',
                source: 'osm-tiles',
                minzoom: 0,
                maxzoom: 19
            }]
        },
        center: [lng, lat],
        zoom: zoom
    });

    // Agregar controles de navegación
    maps[mapId].addControl(new maplibregl.NavigationControl(), 'top-right');

    // Agregar control de escala
    maps[mapId].addControl(new maplibregl.ScaleControl({
        maxWidth: 200,
        unit: 'metric'
    }), 'bottom-left');

    // Inicializar array de marcadores
    markers[mapId] = [];

    // Evento de click en el mapa
    maps[mapId].on('click', (e) => {
        const { lng, lat } = e.lngLat;
        dotNetHelper.invokeMethodAsync('HandleMapClick', lat, lng);
    });

    // Cambiar cursor al pasar por encima
    maps[mapId].on('mouseenter', () => {
        maps[mapId].getCanvas().style.cursor = 'crosshair';
    });
};

window.addMarker = (mapId, lng, lat) => {
    if (!maps[mapId]) return;

    // Limpiar marcadores anteriores
    if (markers[mapId]) {
        markers[mapId].forEach(marker => marker.remove());
        markers[mapId] = [];
    }

    // Crear nuevo marcador
    const marker = new maplibregl.Marker({
        color: '#667eea',
        draggable: false
    })
        .setLngLat([lng, lat])
        .addTo(maps[mapId]);

    markers[mapId].push(marker);

    // Crear popup con información
    const popup = new maplibregl.Popup({ offset: 25 })
        .setHTML(`
            <div style="padding: 10px;">
                <strong>Ubicación Seleccionada</strong><br/>
                <small>Lat: ${lat.toFixed(6)}<br/>Lng: ${lng.toFixed(6)}</small>
            </div>
        `);

    marker.setPopup(popup);
    popup.addTo(maps[mapId]);
};

window.flyToLocation = (mapId, lng, lat, zoom = 18) => {
    if (!maps[mapId]) return;

    maps[mapId].flyTo({
        center: [lng, lat],
        zoom: zoom,
        essential: true,
        duration: 2000
    });
};

window.resizeMap = (mapId) => {
    if (maps[mapId]) {
        setTimeout(() => {
            maps[mapId].resize();
        }, 100);
    }
};

// Limpiar mapa al cerrar
window.removeMap = (mapId) => {
    if (markers[mapId]) {
        markers[mapId].forEach(marker => marker.remove());
        delete markers[mapId];
    }
    if (maps[mapId]) {
        maps[mapId].remove();
        delete maps[mapId];
    }
};
