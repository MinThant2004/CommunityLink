// FiberNeonGlobe — Three.js interactive globe animation with Continental Coastlines
// Adapted from FiberNeonGlobe3.txt for Blazor JS Interop

let _globeRenderer = null;
let _globeAnimationId = null;
let _globeResizeHandler = null;

window.initGlobe = function () {
    // Prevent double-init
    if (_globeRenderer) {
        window.disposeGlobe();
    }

    const container = document.getElementById('globe-canvas');
    if (!container || typeof THREE === 'undefined') return;

    const width = container.clientWidth || window.innerWidth;
    const height = container.clientHeight || 560;

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
    camera.position.set(0, 2, 22);

    const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true, powerPreference: "high-performance" });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setClearColor(0x000000, 0);
    container.appendChild(renderer.domElement);
    _globeRenderer = renderer;

    const globeGroup = new THREE.Group();
    scene.add(globeGroup);

    // Soft ambient & balanced directional lighting
    const ambientLight = new THREE.AmbientLight(0xdbeafe, 0.85);
    scene.add(ambientLight);

    const dirLight1 = new THREE.DirectionalLight(0x38bdf8, 1.3);
    dirLight1.position.set(12, 10, 15);
    scene.add(dirLight1);

    const dirLight2 = new THREE.DirectionalLight(0xd4af37, 0.9);
    dirLight2.position.set(-15, -8, -10);
    scene.add(dirLight2);

    // 1. Sleek Glass Inner Core Sphere with Soft Depth
    const sphereRadius = 6.2;
    const sphereGeo = new THREE.SphereGeometry(sphereRadius, 64, 64);
    const sphereMat = new THREE.MeshPhongMaterial({
        color: 0x0a1120,
        emissive: 0x060c18,
        shininess: 35,
        transparent: true,
        opacity: 0.88
    });
    const coreGlobe = new THREE.Mesh(sphereGeo, sphereMat);
    globeGroup.add(coreGlobe);

    // Subtle Glass Atmosphere Rim
    const glowGeo = new THREE.SphereGeometry(sphereRadius + 0.04, 48, 48);
    const glowMat = new THREE.MeshBasicMaterial({
        color: 0x38bdf8,
        transparent: true,
        opacity: 0.08,
        side: THREE.BackSide
    });
    globeGroup.add(new THREE.Mesh(glowGeo, glowMat));

    // Utility: Lat/Lon to 3D Cartesian coordinates on sphere
    function latLonToVector3(lat, lon, radius) {
        const phi = (90 - lat) * (Math.PI / 180);
        const theta = (lon + 180) * (Math.PI / 180);
        const x = -(radius * Math.sin(phi) * Math.cos(theta));
        const z = radius * Math.sin(phi) * Math.sin(theta);
        const y = radius * Math.cos(phi);
        return new THREE.Vector3(x, y, z);
    }

    // 2. Soft Tactile Geographical Maplines & Continental Coastlines (Surface Contours)
    const continentOutlines = [
        // North America
        [
            [70, -160], [71, -130], [68, -100], [62, -75], [58, -60], [47, -53], [44, -64], [35, -75], [25, -80],
            [29, -90], [26, -97], [18, -95], [15, -92], [9, -79], [13, -87], [20, -105], [28, -112], [34, -119],
            [48, -124], [58, -136], [60, -148], [65, -168], [70, -160]
        ],
        // South America
        [
            [11, -75], [8, -60], [4, -51], [-3, -40], [-10, -36], [-22, -41], [-34, -53], [-46, -65], [-54, -68],
            [-50, -74], [-40, -73], [-20, -70], [-5, -80], [4, -77], [11, -75]
        ],
        // Europe & Mediterranean
        [
            [71, 28], [60, 25], [55, 12], [54, 5], [48, -4], [43, -9], [37, -9], [36, -5], [38, 0], [43, 4],
            [40, 18], [37, 23], [41, 29], [46, 30], [46, 38], [55, 38], [65, 40], [70, 32], [71, 28]
        ],
        // British Isles
        [
            [58, -5], [54, 0], [51, 1], [50, -5], [54, -4], [58, -5]
        ],
        // Africa
        [
            [36, -5], [37, 10], [32, 25], [31, 32], [22, 37], [12, 44], [12, 51], [5, 48], [-11, 40], [-26, 33],
            [-34, 26], [-34, 18], [-22, 14], [-12, 13], [4, 9], [5, 1], [4, -7], [12, -16], [21, -17], [32, -9], [36, -5]
        ],
        // Asia Main Coastline & Subcontinents
        [
            [75, 105], [72, 135], [70, 175], [60, 165], [52, 142], [42, 131], [38, 122], [30, 122], [22, 114],
            [16, 108], [10, 105], [3, 102], [1, 104], [10, 99], [16, 96], [21, 91], [22, 88], [15, 80], [8, 77],
            [13, 74], [21, 70], [25, 62], [25, 57], [15, 53], [12, 44], [25, 55], [30, 48], [40, 50], [42, 70],
            [50, 85], [55, 100], [65, 105], [75, 105]
        ],
        // Japan Archipelago
        [
            [44, 145], [40, 140], [35, 135], [33, 130], [35, 133], [39, 142], [44, 145]
        ],
        // Australia
        [
            [-12, 132], [-14, 136], [-12, 142], [-22, 150], [-33, 152], [-38, 145], [-35, 136], [-32, 129],
            [-34, 118], [-26, 113], [-20, 118], [-15, 125], [-12, 132]
        ],
        // Southeast Asia / Maritime Indonesian Island Chain
        [
            [5, 96], [0, 102], [-5, 106], [-7, 112], [-8, 116], [-8, 122], [-4, 120], [-1, 117], [4, 118], [6, 115], [5, 96]
        ],
        // Philippines
        [
            [18, 121], [14, 124], [7, 125], [10, 122], [15, 120], [18, 121]
        ],
        // Scandinavia
        [
            [71, 26], [68, 15], [62, 5], [58, 8], [56, 13], [60, 18], [65, 23], [71, 26]
        ]
    ];

    // Draw soft glowing maplines right on the surface of the globe
    const mapLineMat = new THREE.LineBasicMaterial({
        color: 0x38bdf8,
        transparent: true,
        opacity: 0.38,
        linewidth: 1.5
    });

    continentOutlines.forEach(polygon => {
        const pts = [];
        for (let i = 0; i < polygon.length - 1; i++) {
            const p1 = polygon[i];
            const p2 = polygon[i + 1];
            const steps = 6;
            for (let s = 0; s < steps; s++) {
                const t = s / steps;
                const lat = p1[0] + (p2[0] - p1[0]) * t;
                const lon = p1[1] + (p2[1] - p1[1]) * t;
                pts.push(latLonToVector3(lat, lon, sphereRadius + 0.035));
            }
        }
        const lastPt = polygon[polygon.length - 1];
        pts.push(latLonToVector3(lastPt[0], lastPt[1], sphereRadius + 0.035));

        const lineGeo = new THREE.BufferGeometry().setFromPoints(pts);
        const lineMesh = new THREE.Line(lineGeo, mapLineMat);
        globeGroup.add(lineMesh);
    });

    // 3. Subtle Meridian & Latitude Wireframe Grid
    const wireMat = new THREE.LineBasicMaterial({
        color: 0x38bdf8,
        transparent: true,
        opacity: 0.12
    });

    // Latitude rings
    for (let lat = -70; lat <= 70; lat += 20) {
        const phi = (90 - lat) * (Math.PI / 180);
        const r = sphereRadius * Math.sin(phi);
        const y = sphereRadius * Math.cos(phi);
        const circleGeo = new THREE.BufferGeometry();
        const pts = [];
        for (let i = 0; i <= 64; i++) {
            const theta = (i / 64) * Math.PI * 2;
            pts.push(new THREE.Vector3(r * Math.cos(theta), y, r * Math.sin(theta)));
        }
        circleGeo.setFromPoints(pts);
        const ring = new THREE.Line(circleGeo, wireMat);
        globeGroup.add(ring);
    }

    // Longitude meridians
    for (let lon = 0; lon < 180; lon += 30) {
        const circleGeo = new THREE.BufferGeometry();
        const pts = [];
        for (let i = 0; i <= 64; i++) {
            const theta = (i / 64) * Math.PI * 2;
            const rot = lon * (Math.PI / 180);
            const x = sphereRadius * Math.sin(theta) * Math.cos(rot);
            const y = sphereRadius * Math.cos(theta);
            const z = sphereRadius * Math.sin(theta) * Math.sin(rot);
            pts.push(new THREE.Vector3(x, y, z));
        }
        circleGeo.setFromPoints(pts);
        const meridian = new THREE.Line(circleGeo, wireMat);
        globeGroup.add(meridian);
    }

    // 4. Soft Continental Point Cloud Clusters
    const particlePts = [];
    const particleColors = [];

    const landmasses = [
        { lat: 45, lon: -100, spanLat: 22, spanLon: 35, count: 180 },
        { lat: -15, lon: -55, spanLat: 22, spanLon: 22, count: 130 },
        { lat: 50, lon: 15, spanLat: 14, spanLon: 22, count: 150 },
        { lat: 5, lon: 20, spanLat: 28, spanLon: 22, count: 160 },
        { lat: 35, lon: 95, spanLat: 28, spanLon: 40, count: 240 },
        { lat: -25, lon: 135, spanLat: 14, spanLon: 18, count: 90 },
        { lat: 18, lon: 96, spanLat: 10, spanLon: 10, count: 80 }
    ];

    landmasses.forEach(land => {
        for (let i = 0; i < land.count; i++) {
            const lat = land.lat + (Math.random() - 0.5) * land.spanLat * 2;
            const lon = land.lon + (Math.random() - 0.5) * land.spanLon * 2;
            const v = latLonToVector3(lat, lon, sphereRadius + 0.04);
            particlePts.push(v.x, v.y, v.z);

            const isGold = Math.random() < 0.12;
            if (isGold) {
                particleColors.push(0.83, 0.68, 0.21);
            } else {
                particleColors.push(0.22, 0.74, 0.97);
            }
        }
    });

    const dotGeo = new THREE.BufferGeometry();
    dotGeo.setAttribute('position', new THREE.Float32BufferAttribute(particlePts, 3));
    dotGeo.setAttribute('color', new THREE.Float32BufferAttribute(particleColors, 3));

    const dotMat = new THREE.PointsMaterial({
        size: 0.11,
        vertexColors: true,
        transparent: true,
        opacity: 0.6
    });
    globeGroup.add(new THREE.Points(dotGeo, dotMat));

    // 5. Global Hub Nodes
    const hubs = [
        { name: 'San Francisco', lat: 37.77, lon: -122.41, color: 0x38bdf8 },
        { name: 'Seattle', lat: 47.60, lon: -122.33, color: 0x38bdf8 },
        { name: 'Austin', lat: 30.26, lon: -97.74, color: 0x38bdf8 },
        { name: 'New York', lat: 40.71, lon: -74.00, color: 0x38bdf8 },
        { name: 'Toronto', lat: 43.65, lon: -79.38, color: 0x38bdf8 },
        { name: 'London', lat: 51.50, lon: -0.12, color: 0xd4af37 },
        { name: 'Paris', lat: 48.85, lon: 2.35, color: 0x38bdf8 },
        { name: 'Zurich', lat: 47.37, lon: 8.54, color: 0xd4af37 },
        { name: 'Frankfurt', lat: 50.11, lon: 8.68, color: 0x38bdf8 },
        { name: 'Stockholm', lat: 59.33, lon: 18.06, color: 0x38bdf8 },
        { name: 'Dubai', lat: 25.20, lon: 55.27, color: 0xd4af37 },
        { name: 'Riyadh', lat: 24.71, lon: 46.67, color: 0xd4af37 },
        { name: 'Bengaluru', lat: 12.97, lon: 77.59, color: 0x38bdf8 },
        { name: 'Mumbai', lat: 19.07, lon: 72.87, color: 0x38bdf8 },
        { name: 'Singapore', lat: 1.35, lon: 103.81, color: 0x38bdf8 },
        { name: 'Yangon', lat: 16.86, lon: 96.19, color: 0xd4af37 },
        { name: 'Bangkok', lat: 13.75, lon: 100.50, color: 0x38bdf8 },
        { name: 'Hong Kong', lat: 22.31, lon: 114.16, color: 0x38bdf8 },
        { name: 'Taipei', lat: 25.03, lon: 121.56, color: 0x38bdf8 },
        { name: 'Tokyo', lat: 35.67, lon: 139.65, color: 0x38bdf8 },
        { name: 'Seoul', lat: 37.56, lon: 126.97, color: 0x38bdf8 },
        { name: 'Sydney', lat: -33.86, lon: 151.20, color: 0x38bdf8 },
        { name: 'Melbourne', lat: -37.81, lon: 144.96, color: 0x38bdf8 },
        { name: 'São Paulo', lat: -23.55, lon: -46.63, color: 0x38bdf8 },
        { name: 'Buenos Aires', lat: -34.60, lon: -58.38, color: 0x38bdf8 },
        { name: 'Johannesburg', lat: -26.20, lon: 28.04, color: 0xd4af37 },
        { name: 'Nairobi', lat: -1.29, lon: 36.82, color: 0xd4af37 }
    ];

    const hubMeshes = [];
    hubs.forEach(hub => {
        const pos = latLonToVector3(hub.lat, hub.lon, sphereRadius + 0.08);
        const hubGroup = new THREE.Group();
        hubGroup.position.copy(pos);
        hubGroup.lookAt(new THREE.Vector3(0, 0, 0));

        const nGeo = new THREE.SphereGeometry(0.14, 16, 16);
        const nMat = new THREE.MeshBasicMaterial({ color: hub.color });
        const nMesh = new THREE.Mesh(nGeo, nMat);
        hubGroup.add(nMesh);

        // Subtle pulsing halo ring
        const rGeo = new THREE.RingGeometry(0.22, 0.29, 24);
        const rMat = new THREE.MeshBasicMaterial({
            color: hub.color,
            transparent: true,
            opacity: 0.45,
            side: THREE.DoubleSide
        });
        const rMesh = new THREE.Mesh(rGeo, rMat);
        hubGroup.add(rMesh);

        globeGroup.add(hubGroup);
        hubMeshes.push({ group: hubGroup, ring: rMesh, mat: rMat, basePos: pos });
    });

    // 6. Neon Fiber Lines with Traveling Data Packets
    const connections = [
        ['San Francisco', 'Tokyo'],
        ['San Francisco', 'New York'],
        ['Seattle', 'Tokyo'],
        ['Seattle', 'San Francisco'],
        ['Austin', 'New York'],
        ['Austin', 'San Francisco'],
        ['New York', 'London'],
        ['Toronto', 'London'],
        ['Toronto', 'Frankfurt'],
        ['London', 'Paris'],
        ['London', 'Zurich'],
        ['Paris', 'Zurich'],
        ['Zurich', 'Frankfurt'],
        ['Frankfurt', 'Stockholm'],
        ['Stockholm', 'London'],
        ['Zurich', 'Dubai'],
        ['Frankfurt', 'Dubai'],
        ['London', 'Dubai'],
        ['Dubai', 'Riyadh'],
        ['Dubai', 'Mumbai'],
        ['Dubai', 'Bengaluru'],
        ['Dubai', 'Singapore'],
        ['Riyadh', 'Bengaluru'],
        ['Mumbai', 'Bengaluru'],
        ['Bengaluru', 'Yangon'],
        ['Bengaluru', 'Singapore'],
        ['Yangon', 'Singapore'],
        ['Yangon', 'Bangkok'],
        ['Bangkok', 'Singapore'],
        ['Singapore', 'Hong Kong'],
        ['Singapore', 'Tokyo'],
        ['Singapore', 'Sydney'],
        ['Hong Kong', 'Taipei'],
        ['Hong Kong', 'Tokyo'],
        ['Taipei', 'Tokyo'],
        ['Tokyo', 'Seoul'],
        ['Seoul', 'San Francisco'],
        ['Tokyo', 'Sydney'],
        ['Sydney', 'Melbourne'],
        ['San Francisco', 'Sydney'],
        ['London', 'São Paulo'],
        ['New York', 'São Paulo'],
        ['São Paulo', 'Buenos Aires'],
        ['Zurich', 'Johannesburg'],
        ['Dubai', 'Johannesburg'],
        ['Johannesburg', 'Nairobi'],
        ['Nairobi', 'Dubai'],
        ['Singapore', 'Melbourne'],
        ['Dubai', 'Stockholm'],
        ['Frankfurt', 'Seoul'],
        ['San Francisco', 'Hong Kong'],
        ['London', 'Singapore']
    ];

    const hubMap = {};
    hubs.forEach(h => { hubMap[h.name] = h; });

    const arcCurves = [];
    const arcPackets = [];

    connections.forEach(([fromName, toName], idx) => {
        const h1 = hubMap[fromName];
        const h2 = hubMap[toName];
        if (!h1 || !h2) return;

        const v1 = latLonToVector3(h1.lat, h1.lon, sphereRadius + 0.05);
        const v2 = latLonToVector3(h2.lat, h2.lon, sphereRadius + 0.05);

        const dist = v1.distanceTo(v2);
        const mid = new THREE.Vector3().addVectors(v1, v2).multiplyScalar(0.5);
        const arcAltitude = Math.max(1.06, 1.0 + (dist / (sphereRadius * 2)) * 0.38);
        mid.normalize().multiplyScalar(sphereRadius * arcAltitude);

        const curve = new THREE.QuadraticBezierCurve3(v1, mid, v2);
        arcCurves.push(curve);

        const isGold = (fromName === 'New York' && toName === 'London') ||
            (fromName === 'Singapore' && toName === 'Yangon') ||
            (fromName === 'Yangon' && toName === 'Bangkok') ||
            (fromName === 'Zurich' && toName === 'Dubai') ||
            (fromName === 'Bengaluru' && toName === 'Yangon') ||
            (fromName === 'Dubai' && toName === 'Johannesburg') ||
            (fromName === 'London' && toName === 'Singapore') ||
            (fromName === 'San Francisco' && toName === 'Tokyo');

        const tubeGeo = new THREE.TubeGeometry(curve, 36, 0.02, 6, false);
        const tubeMat = new THREE.MeshBasicMaterial({
            color: isGold ? 0xd4af37 : 0x38bdf8,
            transparent: true,
            opacity: isGold ? 0.65 : 0.42
        });
        globeGroup.add(new THREE.Mesh(tubeGeo, tubeMat));

        if (idx % 2 === 0) {
            const pGeo = new THREE.SphereGeometry(0.048, 8, 8);
            const pMat = new THREE.MeshBasicMaterial({ color: isGold ? 0xfffae0 : 0xffffff });
            const pMesh = new THREE.Mesh(pGeo, pMat);
            globeGroup.add(pMesh);

            arcPackets.push({
                mesh: pMesh,
                curve: curve,
                progress: (idx * 0.07) % 1.0,
                speed: 0.002 + (idx % 4) * 0.0007
            });
        }
    });

    // Initial tilt
    globeGroup.rotation.x = 0.25;
    globeGroup.rotation.y = -0.4;

    // Interactive Drag & Zoom Controls
    let isDragging = false;
    let prevMousePos = { x: 0, y: 0 };
    let targetRotY = globeGroup.rotation.y;
    let targetRotX = globeGroup.rotation.x;
    let autoRotate = true;

    const dom = renderer.domElement;
    dom.style.cursor = 'grab';

    dom.addEventListener('mousedown', (e) => {
        isDragging = true;
        autoRotate = false;
        dom.style.cursor = 'grabbing';
        prevMousePos = { x: e.clientX, y: e.clientY };
    });

    window.addEventListener('mouseup', () => {
        if (isDragging) {
            isDragging = false;
            dom.style.cursor = 'grab';
        }
    });

    window.addEventListener('mousemove', (e) => {
        if (!isDragging) return;
        const deltaX = e.clientX - prevMousePos.x;
        const deltaY = e.clientY - prevMousePos.y;

        targetRotY += deltaX * 0.007;
        targetRotX += deltaY * 0.007;
        targetRotX = Math.max(-1.1, Math.min(1.1, targetRotX));

        prevMousePos = { x: e.clientX, y: e.clientY };
    });

    // Touch controls
    dom.addEventListener('touchstart', (e) => {
        if (e.touches.length === 1) {
            isDragging = true;
            autoRotate = false;
            prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
        }
    }, { passive: true });

    window.addEventListener('touchmove', (e) => {
        if (!isDragging || e.touches.length !== 1) return;
        const deltaX = e.touches[0].clientX - prevMousePos.x;
        const deltaY = e.touches[0].clientY - prevMousePos.y;

        targetRotY += deltaX * 0.007;
        targetRotX += deltaY * 0.007;
        targetRotX = Math.max(-1.1, Math.min(1.1, targetRotX));

        prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
    }, { passive: true });

    window.addEventListener('touchend', () => {
        isDragging = false;
    });

    // Wheel to Zoom
    dom.addEventListener('wheel', (e) => {
        e.preventDefault();
        camera.position.z += e.deltaY * 0.012;
        camera.position.z = Math.max(14, Math.min(32, camera.position.z));
    }, { passive: false });

    // Animation Loop
    let time = 0;
    function animate() {
        _globeAnimationId = requestAnimationFrame(animate);
        time += 0.015;

        // Rotation dampening
        globeGroup.rotation.y += (targetRotY - globeGroup.rotation.y) * 0.08;
        globeGroup.rotation.x += (targetRotX - globeGroup.rotation.x) * 0.08;

        if (autoRotate) {
            targetRotY += 0.0018;
        }

        // Pulse hub halos
        hubMeshes.forEach((h, i) => {
            const scale = 1 + Math.sin(time * 2.5 + i) * 0.2;
            h.ring.scale.set(scale, scale, 1);
            h.mat.opacity = 0.28 + Math.sin(time * 2.5 + i) * 0.22;
        });

        // Flow packets along arcs
        arcPackets.forEach(p => {
            p.progress = (p.progress + p.speed) % 1.0;
            const pt = p.curve.getPoint(p.progress);
            p.mesh.position.copy(pt);
        });

        renderer.render(scene, camera);
    }
    animate();

    // Resize handling
    _globeResizeHandler = function () {
        const newW = container.clientWidth || window.innerWidth;
        const newH = container.clientHeight || 560;
        camera.aspect = newW / newH;
        camera.updateProjectionMatrix();
        renderer.setSize(newW, newH);
    };
    window.addEventListener('resize', _globeResizeHandler);
};

window.disposeGlobe = function () {
    if (_globeAnimationId) {
        cancelAnimationFrame(_globeAnimationId);
        _globeAnimationId = null;
    }
    if (_globeResizeHandler) {
        window.removeEventListener('resize', _globeResizeHandler);
        _globeResizeHandler = null;
    }
    if (_globeRenderer) {
        _globeRenderer.dispose();
        const canvas = _globeRenderer.domElement;
        if (canvas && canvas.parentNode) {
            canvas.parentNode.removeChild(canvas);
        }
        _globeRenderer = null;
    }
};
