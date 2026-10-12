// FiberNeonGlobe — High-performance Three.js interactive globe animation
// Adapted for Blazor JS Interop with full lifecycle management

let _globeRenderer = null;
let _globeAnimationId = null;
let _globeResizeHandler = null;
let _globeCleanups = [];

window.initGlobe = function () {
    // Prevent double-init
    if (_globeRenderer) {
        window.disposeGlobe();
    }

    const container = document.getElementById('globe-canvas') || document.getElementById('threejs-container');
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

    // Soft, balanced ambient & directional lighting
    const ambientLight = new THREE.AmbientLight(0xdbeafe, 0.75);
    scene.add(ambientLight);

    const dirLight1 = new THREE.DirectionalLight(0x38bdf8, 1.2);
    dirLight1.position.set(12, 10, 15);
    scene.add(dirLight1);

    const dirLight2 = new THREE.DirectionalLight(0xd4af37, 0.85);
    dirLight2.position.set(-15, -8, -10);
    scene.add(dirLight2);

    // 1. Sleek Translucent Inner Core Sphere
    const sphereRadius = 6.2;
    const sphereGeo = new THREE.SphereGeometry(sphereRadius, 64, 64);
    const sphereMat = new THREE.MeshPhongMaterial({
        color: 0x0c1427,
        emissive: 0x081020,
        shininess: 40,
        transparent: true,
        opacity: 0.85
    });
    const coreGlobe = new THREE.Mesh(sphereGeo, sphereMat);
    globeGroup.add(coreGlobe);

    // 2. Minimal Wireframe Latitude & Longitude Meridian Grid
    const wireMat = new THREE.LineBasicMaterial({
        color: 0x38bdf8,
        transparent: true,
        opacity: 0.16
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

    // Longitude rings
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

    // Utility: Lat/Lon to 3D Cartesian coordinates
    function latLonToVector3(lat, lon, radius) {
        const phi = (90 - lat) * (Math.PI / 180);
        const theta = (lon + 180) * (Math.PI / 180);
        const x = -(radius * Math.sin(phi) * Math.cos(theta));
        const z = radius * Math.sin(phi) * Math.sin(theta);
        const y = radius * Math.cos(phi);
        return new THREE.Vector3(x, y, z);
    }

    // Global Hub Nodes
    const hubs = [
        // North America
        { name: 'San Francisco', lat: 37.77, lon: -122.41, color: 0x38bdf8 },
        { name: 'Los Angeles', lat: 34.05, lon: -118.24, color: 0x38bdf8 },
        { name: 'Seattle', lat: 47.60, lon: -122.33, color: 0x38bdf8 },
        { name: 'Austin', lat: 30.26, lon: -97.74, color: 0x38bdf8 },
        { name: 'Chicago', lat: 41.87, lon: -87.62, color: 0x38bdf8 },
        { name: 'New York', lat: 40.71, lon: -74.00, color: 0x38bdf8 },
        { name: 'Miami', lat: 25.76, lon: -80.19, color: 0x38bdf8 },
        { name: 'Toronto', lat: 43.65, lon: -79.38, color: 0x38bdf8 },
        { name: 'Vancouver', lat: 49.28, lon: -123.12, color: 0x38bdf8 },
        { name: 'Honolulu', lat: 21.30, lon: -157.85, color: 0x38bdf8 },

        // Europe
        { name: 'London', lat: 51.50, lon: -0.12, color: 0xd4af37 },
        { name: 'Paris', lat: 48.85, lon: 2.35, color: 0x38bdf8 },
        { name: 'Amsterdam', lat: 52.37, lon: 4.90, color: 0x38bdf8 },
        { name: 'Frankfurt', lat: 50.11, lon: 8.68, color: 0x38bdf8 },
        { name: 'Berlin', lat: 52.52, lon: 13.40, color: 0x38bdf8 },
        { name: 'Zurich', lat: 47.37, lon: 8.54, color: 0xd4af37 },
        { name: 'Stockholm', lat: 59.33, lon: 18.06, color: 0x38bdf8 },
        { name: 'Madrid', lat: 40.41, lon: -3.70, color: 0x38bdf8 },

        // Middle East & Africa
        { name: 'Dubai', lat: 25.20, lon: 55.27, color: 0xd4af37 },
        { name: 'Riyadh', lat: 24.71, lon: 46.67, color: 0xd4af37 },
        { name: 'Doha', lat: 25.28, lon: 51.53, color: 0xd4af37 },
        { name: 'Cairo', lat: 30.04, lon: 31.23, color: 0xd4af37 },
        { name: 'Istanbul', lat: 41.01, lon: 28.97, color: 0x38bdf8 },
        { name: 'Johannesburg', lat: -26.20, lon: 28.04, color: 0xd4af37 },
        { name: 'Nairobi', lat: -1.29, lon: 36.82, color: 0xd4af37 },

        // Asia
        { name: 'Bengaluru', lat: 12.97, lon: 77.59, color: 0x38bdf8 },
        { name: 'Mumbai', lat: 19.07, lon: 72.87, color: 0x38bdf8 },
        { name: 'Singapore', lat: 1.35, lon: 103.81, color: 0x38bdf8 },
        { name: 'Yangon', lat: 16.86, lon: 96.19, color: 0xd4af37 },
        { name: 'Bangkok', lat: 13.75, lon: 100.50, color: 0x38bdf8 },
        { name: 'Jakarta', lat: -6.20, lon: 106.84, color: 0x38bdf8 },
        { name: 'Manila', lat: 14.59, lon: 120.98, color: 0x38bdf8 },
        { name: 'Hong Kong', lat: 22.31, lon: 114.16, color: 0x38bdf8 },
        { name: 'Taipei', lat: 25.03, lon: 121.56, color: 0x38bdf8 },
        { name: 'Tokyo', lat: 35.67, lon: 139.65, color: 0x38bdf8 },
        { name: 'Seoul', lat: 37.56, lon: 126.97, color: 0x38bdf8 },

        // Oceania
        { name: 'Sydney', lat: -33.86, lon: 151.20, color: 0x38bdf8 },
        { name: 'Melbourne', lat: -37.81, lon: 144.96, color: 0x38bdf8 },
        { name: 'Auckland', lat: -36.84, lon: 174.76, color: 0x38bdf8 },

        // South & Latin America
        { name: 'São Paulo', lat: -23.55, lon: -46.63, color: 0x38bdf8 },
        { name: 'Buenos Aires', lat: -34.60, lon: -58.38, color: 0x38bdf8 },
        { name: 'Santiago', lat: -33.44, lon: -70.66, color: 0x38bdf8 },
        { name: 'Bogota', lat: 4.71, lon: -74.07, color: 0x38bdf8 }
    ];

    // Minimal continent point cloud clusters
    const particlePts = [];
    const particleColors = [];

    const landmasses = [
        { lat: 45, lon: -100, spanLat: 25, spanLon: 40, count: 200 }, // North America
        { lat: -15, lon: -55, spanLat: 25, spanLon: 25, count: 140 }, // South America
        { lat: 50, lon: 15, spanLat: 15, spanLon: 25, count: 160 },  // Europe
        { lat: 5, lon: 20, spanLat: 30, spanLon: 25, count: 180 },   // Africa
        { lat: 35, lon: 95, spanLat: 30, spanLon: 45, count: 280 },  // Asia
        { lat: -25, lon: 135, spanLat: 15, spanLon: 20, count: 100 }, // Australia
        { lat: 18, lon: 96, spanLat: 10, spanLon: 10, count: 80 }    // SE Asia / Myanmar
    ];

    landmasses.forEach(land => {
        for (let i = 0; i < land.count; i++) {
            const lat = land.lat + (Math.random() - 0.5) * land.spanLat * 2;
            const lon = land.lon + (Math.random() - 0.5) * land.spanLon * 2;
            const v = latLonToVector3(lat, lon, sphereRadius + 0.03);
            particlePts.push(v.x, v.y, v.z);

            const isGold = Math.random() < 0.12;
            if (isGold) {
                particleColors.push(0.83, 0.68, 0.21); // #d4af37
            } else {
                particleColors.push(0.22, 0.74, 0.97); // #38bdf8
            }
        }
    });

    const dotGeo = new THREE.BufferGeometry();
    dotGeo.setAttribute('position', new THREE.Float32BufferAttribute(particlePts, 3));
    dotGeo.setAttribute('color', new THREE.Float32BufferAttribute(particleColors, 3));

    const dotMat = new THREE.PointsMaterial({
        size: 0.12,
        vertexColors: true,
        transparent: true,
        opacity: 0.65
    });
    const dotParticles = new THREE.Points(dotGeo, dotMat);
    globeGroup.add(dotParticles);

    // 4. Hub Nodes (Clean small spheres + tiny halo rings)
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

    // 5. Enriched, Well-Distributed Neon Fiber Lines
    const connections = [
        // North America Intercity Mesh
        ['San Francisco', 'Seattle'],
        ['San Francisco', 'Los Angeles'],
        ['San Francisco', 'Austin'],
        ['San Francisco', 'Chicago'],
        ['San Francisco', 'New York'],
        ['Los Angeles', 'Seattle'],
        ['Los Angeles', 'Austin'],
        ['Los Angeles', 'Chicago'],
        ['Los Angeles', 'Honolulu'],
        ['Seattle', 'Vancouver'],
        ['Vancouver', 'Toronto'],
        ['Vancouver', 'San Francisco'],
        ['Austin', 'Chicago'],
        ['Austin', 'Miami'],
        ['Austin', 'New York'],
        ['Chicago', 'Toronto'],
        ['Chicago', 'New York'],
        ['New York', 'Toronto'],
        ['New York', 'Miami'],
        ['Miami', 'Bogota'],

        // Trans-Atlantic Trunks
        ['New York', 'London'],
        ['New York', 'Paris'],
        ['New York', 'Frankfurt'],
        ['New York', 'Amsterdam'],
        ['Toronto', 'London'],
        ['Toronto', 'Frankfurt'],
        ['Miami', 'Madrid'],
        ['Miami', 'London'],

        // European Mesh
        ['London', 'Paris'],
        ['London', 'Amsterdam'],
        ['London', 'Frankfurt'],
        ['London', 'Zurich'],
        ['London', 'Stockholm'],
        ['Paris', 'Amsterdam'],
        ['Paris', 'Frankfurt'],
        ['Paris', 'Zurich'],
        ['Paris', 'Madrid'],
        ['Amsterdam', 'Berlin'],
        ['Amsterdam', 'Frankfurt'],
        ['Berlin', 'Frankfurt'],
        ['Berlin', 'Stockholm'],
        ['Frankfurt', 'Zurich'],
        ['Frankfurt', 'Stockholm'],
        ['Frankfurt', 'Istanbul'],
        ['Zurich', 'Madrid'],
        ['Madrid', 'London'],

        // Europe <-> Middle East & Africa
        ['London', 'Dubai'],
        ['Paris', 'Dubai'],
        ['Frankfurt', 'Dubai'],
        ['Zurich', 'Dubai'],
        ['Stockholm', 'Dubai'],
        ['Amsterdam', 'Dubai'],
        ['Istanbul', 'Dubai'],
        ['Istanbul', 'Cairo'],
        ['Cairo', 'Dubai'],
        ['Cairo', 'Riyadh'],
        ['London', 'Johannesburg'],
        ['Paris', 'Johannesburg'],
        ['Zurich', 'Johannesburg'],
        ['Dubai', 'Johannesburg'],
        ['Dubai', 'Riyadh'],
        ['Dubai', 'Doha'],
        ['Riyadh', 'Doha'],
        ['Johannesburg', 'Nairobi'],
        ['Nairobi', 'Dubai'],
        ['Nairobi', 'Cairo'],

        // Middle East <-> South & Southeast Asia
        ['Dubai', 'Mumbai'],
        ['Dubai', 'Bengaluru'],
        ['Dubai', 'Singapore'],
        ['Doha', 'Mumbai'],
        ['Riyadh', 'Bengaluru'],
        ['Riyadh', 'Mumbai'],
        ['Mumbai', 'Bengaluru'],
        ['Bengaluru', 'Yangon'],
        ['Bengaluru', 'Singapore'],
        ['Bengaluru', 'Bangkok'],
        ['Mumbai', 'Singapore'],
        ['Mumbai', 'Yangon'],

        // Southeast Asia & East Asia Regional Trunks
        ['Yangon', 'Bangkok'],
        ['Yangon', 'Singapore'],
        ['Bangkok', 'Singapore'],
        ['Bangkok', 'Hong Kong'],
        ['Bangkok', 'Jakarta'],
        ['Singapore', 'Jakarta'],
        ['Singapore', 'Manila'],
        ['Singapore', 'Hong Kong'],
        ['Singapore', 'Taipei'],
        ['Singapore', 'Tokyo'],
        ['Jakarta', 'Manila'],
        ['Jakarta', 'Sydney'],
        ['Manila', 'Hong Kong'],
        ['Manila', 'Taipei'],
        ['Manila', 'Tokyo'],
        ['Hong Kong', 'Taipei'],
        ['Hong Kong', 'Seoul'],
        ['Hong Kong', 'Tokyo'],
        ['Taipei', 'Tokyo'],
        ['Taipei', 'Seoul'],
        ['Seoul', 'Tokyo'],

        // Trans-Pacific Trunks (Asia -> Americas)
        ['Tokyo', 'Seattle'],
        ['Tokyo', 'San Francisco'],
        ['Tokyo', 'Los Angeles'],
        ['Tokyo', 'Vancouver'],
        ['Tokyo', 'Honolulu'],
        ['Honolulu', 'San Francisco'],
        ['Honolulu', 'Sydney'],
        ['Seoul', 'San Francisco'],
        ['Seoul', 'Seattle'],
        ['Hong Kong', 'San Francisco'],
        ['Taipei', 'San Francisco'],
        ['Singapore', 'San Francisco'],

        // Asia-Pacific & Oceania Trunks
        ['Tokyo', 'Sydney'],
        ['Singapore', 'Sydney'],
        ['Singapore', 'Melbourne'],
        ['Manila', 'Sydney'],
        ['Sydney', 'Melbourne'],
        ['Sydney', 'Auckland'],
        ['Melbourne', 'Auckland'],

        // Latin America & South Atlantic Trunks
        ['Miami', 'São Paulo'],
        ['New York', 'São Paulo'],
        ['Los Angeles', 'Bogota'],
        ['Bogota', 'Santiago'],
        ['Bogota', 'São Paulo'],
        ['São Paulo', 'Buenos Aires'],
        ['São Paulo', 'Santiago'],
        ['Buenos Aires', 'Santiago'],
        ['São Paulo', 'London'],
        ['São Paulo', 'Madrid'],
        ['São Paulo', 'Johannesburg'],
        ['Buenos Aires', 'Johannesburg'],
        ['Santiago', 'Sydney'],
        ['Auckland', 'Santiago'],

        // Transcontinental Global Highways (Europe -> Asia)
        ['Frankfurt', 'Seoul'],
        ['Frankfurt', 'Tokyo'],
        ['London', 'Singapore'],
        ['Zurich', 'Singapore'],
        ['Dubai', 'Tokyo']
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
        // Smooth parabolic altitude based on distance
        const arcAltitude = Math.max(1.06, 1.0 + (dist / (sphereRadius * 2)) * 0.38);
        mid.normalize().multiplyScalar(sphereRadius * arcAltitude);

        const curve = new THREE.QuadraticBezierCurve3(v1, mid, v2);
        arcCurves.push(curve);

        // Sovereign direct routes highlighted in gold, balance in luminous cyan
        const isGold = (fromName === 'New York' && toName === 'London') ||
                       (fromName === 'Singapore' && toName === 'Yangon') ||
                       (fromName === 'Yangon' && toName === 'Bangkok') ||
                       (fromName === 'Zurich' && toName === 'Dubai') ||
                       (fromName === 'Bengaluru' && toName === 'Yangon') ||
                       (fromName === 'Dubai' && toName === 'Johannesburg') ||
                       (fromName === 'London' && toName === 'Singapore') ||
                       (fromName === 'San Francisco' && toName === 'Tokyo') ||
                       (fromName === 'Hong Kong' && toName === 'Tokyo') ||
                       (fromName === 'São Paulo' && toName === 'London');

        // Refined slender neon fiber line tube
        const tubeGeo = new THREE.TubeGeometry(curve, 36, 0.022, 6, false);
        const tubeMat = new THREE.MeshBasicMaterial({
            color: isGold ? 0xd4af37 : 0x38bdf8,
            transparent: true,
            opacity: isGold ? 0.75 : 0.52
        });
        const tubeMesh = new THREE.Mesh(tubeGeo, tubeMat);
        globeGroup.add(tubeMesh);

        // Subtle moving packet on key lines (every 2nd connection to prevent visual clutter)
        if (idx % 2 === 0) {
            const pGeo = new THREE.SphereGeometry(0.05, 8, 8);
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

    const onMouseDown = (e) => {
        isDragging = true;
        autoRotate = false;
        dom.style.cursor = 'grabbing';
        prevMousePos = { x: e.clientX, y: e.clientY };
    };

    const onMouseUp = () => {
        if (isDragging) {
            isDragging = false;
            dom.style.cursor = 'grab';
        }
    };

    const onMouseMove = (e) => {
        if (!isDragging) return;
        const deltaX = e.clientX - prevMousePos.x;
        const deltaY = e.clientY - prevMousePos.y;

        targetRotY += deltaX * 0.007;
        targetRotX += deltaY * 0.007;
        targetRotX = Math.max(-1.1, Math.min(1.1, targetRotX));

        prevMousePos = { x: e.clientX, y: e.clientY };
    };

    const onTouchStart = (e) => {
        if (e.touches.length === 1) {
            isDragging = true;
            autoRotate = false;
            prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
        }
    };

    const onTouchMove = (e) => {
        if (!isDragging || e.touches.length !== 1) return;
        const deltaX = e.touches[0].clientX - prevMousePos.x;
        const deltaY = e.touches[0].clientY - prevMousePos.y;

        targetRotY += deltaX * 0.007;
        targetRotX += deltaY * 0.007;
        targetRotX = Math.max(-1.1, Math.min(1.1, targetRotX));

        prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
    };

    const onTouchEnd = () => {
        isDragging = false;
    };

    const onWheel = (e) => {
        e.preventDefault();
        camera.position.z += e.deltaY * 0.012;
        camera.position.z = Math.max(14, Math.min(32, camera.position.z));
    };

    dom.addEventListener('mousedown', onMouseDown);
    window.addEventListener('mouseup', onMouseUp);
    window.addEventListener('mousemove', onMouseMove);
    dom.addEventListener('touchstart', onTouchStart, { passive: true });
    window.addEventListener('touchmove', onTouchMove, { passive: true });
    window.addEventListener('touchend', onTouchEnd);
    dom.addEventListener('wheel', onWheel, { passive: false });

    _globeCleanups.push(() => {
        dom.removeEventListener('mousedown', onMouseDown);
        window.removeEventListener('mouseup', onMouseUp);
        window.removeEventListener('mousemove', onMouseMove);
        dom.removeEventListener('touchstart', onTouchStart);
        window.removeEventListener('touchmove', onTouchMove);
        window.removeEventListener('touchend', onTouchEnd);
        dom.removeEventListener('wheel', onWheel);
    });

    // Animation Loop
    let time = 0;
    function animate() {
        _globeAnimationId = requestAnimationFrame(animate);
        time += 0.015;

        // Smooth rotation dampening
        globeGroup.rotation.y += (targetRotY - globeGroup.rotation.y) * 0.08;
        globeGroup.rotation.x += (targetRotX - globeGroup.rotation.x) * 0.08;

        if (autoRotate) {
            targetRotY += 0.0018;
        }

        // Gentle pulse for hub halos
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
    while (_globeCleanups.length > 0) {
        const cleanup = _globeCleanups.pop();
        try { cleanup(); } catch (e) { }
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
