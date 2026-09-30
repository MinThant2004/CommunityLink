// LinkDrop 3D Rhombic Diamond Prism & Neon Orbital Rings
// High-performance shared WebGL engine for CommunityLink
// Supports single or multi-instance display (navbar, tables, badges, modals) with 0 context limit issues

(function () {
    let _scene = null;
    let _camera = null;
    let _masterRenderer = null;
    let _masterCanvas = null;
    let _masterGroup = null;
    let _prismGroup = null;
    let _haloMesh = null;
    let _neonBloomMesh = null;
    let _neonRingGroup = null;
    let _secRingMesh = null;
    let _clock = null;
    let _animId = null;

    let _isDragging = false;
    let _hasMoved = false;
    let _prevMousePos = { x: 0, y: 0 };
    let _listenersAttached = false;

    const _instances = new Map();

    const MASTER_SIZE = 384;

    function ensureMasterScene() {
        if (_masterRenderer) return true;
        if (typeof THREE === 'undefined') return false;

        try {
            _masterCanvas = document.createElement('canvas');
            _masterCanvas.width = MASTER_SIZE;
            _masterCanvas.height = MASTER_SIZE;

            _scene = new THREE.Scene();
            _camera = new THREE.PerspectiveCamera(45, 1, 0.1, 1000);
            _camera.position.set(0, 0, 8.5);

            _masterRenderer = new THREE.WebGLRenderer({
                canvas: _masterCanvas,
                alpha: true,
                antialias: true,
                preserveDrawingBuffer: true,
                powerPreference: 'high-performance'
            });
            _masterRenderer.setSize(MASTER_SIZE, MASTER_SIZE, false);
            _masterRenderer.setPixelRatio(1);
            _masterRenderer.setClearColor(0x000000, 0);

            // Lighting
            const ambientLight = new THREE.AmbientLight(0xffffff, 0.95);
            _scene.add(ambientLight);

            const dirLight1 = new THREE.DirectionalLight(0x38bdf8, 2.8);
            dirLight1.position.set(5, 8, 6);
            _scene.add(dirLight1);

            const dirLight2 = new THREE.DirectionalLight(0x0284c7, 2.2);
            dirLight2.position.set(-6, -4, -4);
            _scene.add(dirLight2);

            const pointLight = new THREE.PointLight(0x00f0ff, 4.0, 15);
            pointLight.position.set(0, 0, 2);
            _scene.add(pointLight);

            // Master Group holding prism and neon rings
            _masterGroup = new THREE.Group();
            _scene.add(_masterGroup);

            // Internal Prism Group
            _prismGroup = new THREE.Group();
            _masterGroup.add(_prismGroup);

            // Rhombic Bipyramid Geometry (Pure 3D Diamond Prism)
            const vertices = new Float32Array([
                // Top-Front-Right
                0, 2.6, 0,   1.8, 0, 0,   0, 0, 1.1,
                // Top-Front-Left
                0, 2.6, 0,   0, 0, 1.1,   -1.8, 0, 0,
                // Top-Back-Left
                0, 2.6, 0,   -1.8, 0, 0,  0, 0, -1.1,
                // Top-Back-Right
                0, 2.6, 0,   0, 0, -1.1,  1.8, 0, 0,

                // Bottom-Front-Right
                0, -2.6, 0,  0, 0, 1.1,   1.8, 0, 0,
                // Bottom-Front-Left
                0, -2.6, 0,  -1.8, 0, 0,  0, 0, 1.1,
                // Bottom-Back-Left
                0, -2.6, 0,  0, 0, -1.1,  -1.8, 0, 0,
                // Bottom-Back-Right
                0, -2.6, 0,  1.8, 0, 0,   0, 0, -1.1,
            ]);

            const prismGeo = new THREE.BufferGeometry();
            prismGeo.setAttribute('position', new THREE.BufferAttribute(vertices, 3));
            prismGeo.computeVertexNormals();

            // Translucent Crystalline Material
            const prismMat = new THREE.MeshPhongMaterial({
                color: 0x0284c7,
                emissive: 0x0369a1,
                specular: 0xffffff,
                shininess: 95,
                transparent: true,
                opacity: 0.72,
                side: THREE.DoubleSide,
                depthWrite: false
            });

            const prismMesh = new THREE.Mesh(prismGeo, prismMat);
            _prismGroup.add(prismMesh);

            // Glowing Edge Highlights
            const wireMat = new THREE.LineBasicMaterial({
                color: 0xa5f3fc,
                linewidth: 2,
                transparent: true,
                opacity: 0.95
            });
            const wireframeGeo = new THREE.WireframeGeometry(prismGeo);
            const wireframe = new THREE.LineSegments(wireframeGeo, wireMat);
            _prismGroup.add(wireframe);

            // Inner Core Glowing Nucleus (Escrow settlement core)
            const coreGeo = new THREE.SphereGeometry(0.36, 32, 32);
            const coreMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
            const coreMesh = new THREE.Mesh(coreGeo, coreMat);
            _prismGroup.add(coreMesh);

            const haloGeo = new THREE.SphereGeometry(0.58, 32, 32);
            const haloMat = new THREE.MeshBasicMaterial({
                color: 0x38bdf8,
                transparent: true,
                opacity: 0.55
            });
            _haloMesh = new THREE.Mesh(haloGeo, haloMat);
            _prismGroup.add(_haloMesh);

            // 6 Outer Quantum Node Pips at Diamond Vertices
            const nodeGeo = new THREE.SphereGeometry(0.12, 16, 16);
            const nodeMat = new THREE.MeshBasicMaterial({ color: 0x67e8f9 });
            const nodePositions = [
                [1.8, 0, 0],
                [-1.8, 0, 0],
                [0, 0, 1.1],
                [0, 0, -1.1],
                [0, 2.6, 0],
                [0, -2.6, 0]
            ];
            nodePositions.forEach(function (pos) {
                const node = new THREE.Mesh(nodeGeo, nodeMat);
                node.position.set(pos[0], pos[1], pos[2]);
                _prismGroup.add(node);
            });

            // Neon Orbital Rings
            _neonRingGroup = new THREE.Group();
            _masterGroup.add(_neonRingGroup);

            // 1. Core bright white-hot wire
            const thinStringGeo = new THREE.TorusGeometry(2.8, 0.022, 24, 128);
            const thinStringMat = new THREE.MeshBasicMaterial({
                color: 0xffffff,
                transparent: true,
                opacity: 0.95
            });
            const thinStringMesh = new THREE.Mesh(thinStringGeo, thinStringMat);
            _neonRingGroup.add(thinStringMesh);

            // 2. Electric cyan bloom tube
            const neonBloomGeo = new THREE.TorusGeometry(2.8, 0.065, 24, 128);
            const neonBloomMat = new THREE.MeshBasicMaterial({
                color: 0x00f0ff,
                transparent: true,
                opacity: 0.65
            });
            _neonBloomMesh = new THREE.Mesh(neonBloomGeo, neonBloomMat);
            _neonRingGroup.add(_neonBloomMesh);

            // 3. Wide diffuse aura halo
            const neonAuraGeo = new THREE.TorusGeometry(2.8, 0.14, 16, 128);
            const neonAuraMat = new THREE.MeshBasicMaterial({
                color: 0x0284c7,
                transparent: true,
                opacity: 0.28
            });
            const neonAuraMesh = new THREE.Mesh(neonAuraGeo, neonAuraMat);
            _neonRingGroup.add(neonAuraMesh);

            // 4. Secondary orbital neon ring tilted
            const secRingGeo = new THREE.TorusGeometry(3.05, 0.016, 20, 128);
            const secRingMat = new THREE.MeshBasicMaterial({
                color: 0x38bdf8,
                transparent: true,
                opacity: 0.55
            });
            _secRingMesh = new THREE.Mesh(secRingGeo, secRingMat);
            _secRingMesh.rotation.x = Math.PI / 3;
            _secRingMesh.rotation.y = Math.PI / 6;
            _masterGroup.add(_secRingMesh);

            _clock = new THREE.Clock();

            attachGlobalInteractionListeners();
            return true;
        } catch (e) {
            console.error("LinkDrop 3D Scene Initialization failed:", e);
            return false;
        }
    }

    function attachGlobalInteractionListeners() {
        if (_listenersAttached) return;
        _listenersAttached = true;

        window.addEventListener('mousemove', function (e) {
            if (!_isDragging || !_masterGroup) return;
            const deltaX = e.clientX - _prevMousePos.x;
            const deltaY = e.clientY - _prevMousePos.y;
            if (Math.abs(deltaX) > 2 || Math.abs(deltaY) > 2) {
                _hasMoved = true;
            }
            _masterGroup.rotation.y += deltaX * 0.012;
            _masterGroup.rotation.x += deltaY * 0.012;
            _prevMousePos = { x: e.clientX, y: e.clientY };
        });

        window.addEventListener('mouseup', function (e) {
            if (_isDragging && _hasMoved) {
                e.preventDefault();
                e.stopPropagation();
            }
            _isDragging = false;
        });

        window.addEventListener('touchmove', function (e) {
            if (!_isDragging || !_masterGroup || e.touches.length !== 1) return;
            const deltaX = e.touches[0].clientX - _prevMousePos.x;
            const deltaY = e.touches[0].clientY - _prevMousePos.y;
            if (Math.abs(deltaX) > 2 || Math.abs(deltaY) > 2) {
                _hasMoved = true;
            }
            _masterGroup.rotation.y += deltaX * 0.012;
            _masterGroup.rotation.x += deltaY * 0.012;
            _prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
        }, { passive: true });

        window.addEventListener('touchend', function () {
            _isDragging = false;
        });
    }

    function animate() {
        if (_instances.size === 0) {
            _animId = null;
            return;
        }

        _animId = requestAnimationFrame(animate);
        const elapsedTime = _clock ? _clock.getElapsedTime() : 0;

        if (!_isDragging && _prismGroup && _neonRingGroup && _secRingMesh) {
            _prismGroup.rotation.y += 0.014;
            _prismGroup.rotation.x = Math.sin(elapsedTime * 0.7) * 0.15;
            _prismGroup.position.y = Math.sin(elapsedTime * 1.6) * 0.12;

            _neonRingGroup.rotation.x = Math.PI / 2.3 + Math.sin(elapsedTime * 0.5) * 0.12;
            _neonRingGroup.rotation.y += 0.008;
            _secRingMesh.rotation.z += 0.005;
        }

        if (_haloMesh) {
            const pulse = 1 + Math.sin(elapsedTime * 3.8) * 0.16;
            _haloMesh.scale.set(pulse, pulse, pulse);
        }

        if (_neonBloomMesh) {
            const ringPulse = 1 + Math.sin(elapsedTime * 2.5) * 0.06;
            _neonBloomMesh.scale.set(ringPulse, ringPulse, ringPulse);
        }

        if (_masterRenderer && _scene && _camera) {
            _masterRenderer.render(_scene, _camera);

            const src = _masterRenderer.domElement;
            _instances.forEach(function (inst) {
                if (!inst.ctx || !inst.canvas) return;
                const w = inst.canvas.width;
                const h = inst.canvas.height;
                if (w <= 0 || h <= 0) return;
                inst.ctx.clearRect(0, 0, w, h);
                inst.ctx.drawImage(src, 0, 0, w, h);
            });
        }
    }

    function updateInstanceSize(inst) {
        if (!inst || !inst.container || !inst.canvas) return;
        const rect = inst.container.getBoundingClientRect();
        let cssW = rect.width || inst.container.clientWidth || 24;
        let cssH = rect.height || inst.container.clientHeight || 24;
        if (cssW < 12) cssW = 24;
        if (cssH < 12) cssH = 24;

        const dpr = Math.min(window.devicePixelRatio || 1, 2.5);
        const pixelW = Math.round(cssW * dpr);
        const pixelH = Math.round(cssH * dpr);

        if (inst.canvas.width !== pixelW || inst.canvas.height !== pixelH) {
            inst.canvas.width = pixelW;
            inst.canvas.height = pixelH;
            inst.canvas.style.width = '100%';
            inst.canvas.style.height = '100%';
            if (inst.ctx) {
                inst.ctx.imageSmoothingEnabled = true;
                inst.ctx.imageSmoothingQuality = 'high';
            }
        }
    }

    window.initLinkDrop3d = function (containerId) {
        if (!ensureMasterScene()) {
            if (typeof THREE === 'undefined') {
                setTimeout(function () {
                    window.initLinkDrop3d(containerId);
                }, 60);
            }
            return;
        }

        const container = document.getElementById(containerId);
        if (!container) return;

        // Dispose previous instance if re-initializing on the same container
        if (_instances.has(containerId)) {
            window.disposeLinkDrop3d(containerId);
        }

        container.innerHTML = '';

        const canvas = document.createElement('canvas');
        canvas.style.display = 'block';
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        canvas.style.cursor = 'grab';
        canvas.setAttribute('aria-hidden', 'true');
        container.appendChild(canvas);

        const ctx = canvas.getContext('2d', { alpha: true });

        const onMouseDown = function (e) {
            _isDragging = true;
            _hasMoved = false;
            _prevMousePos = { x: e.clientX, y: e.clientY };
        };

        const onTouchStart = function (e) {
            if (e.touches.length === 1) {
                _isDragging = true;
                _hasMoved = false;
                _prevMousePos = { x: e.touches[0].clientX, y: e.touches[0].clientY };
            }
        };

        canvas.addEventListener('mousedown', onMouseDown);
        canvas.addEventListener('touchstart', onTouchStart, { passive: true });

        const inst = {
            container: container,
            canvas: canvas,
            ctx: ctx,
            cleanupListeners: function () {
                canvas.removeEventListener('mousedown', onMouseDown);
                canvas.removeEventListener('touchstart', onTouchStart);
            },
            resizeObserver: null
        };

        updateInstanceSize(inst);

        if (typeof ResizeObserver !== 'undefined') {
            inst.resizeObserver = new ResizeObserver(function (entries) {
                for (let i = 0; i < entries.length; i++) {
                    if (entries[i].contentRect.width > 0 && entries[i].contentRect.height > 0) {
                        updateInstanceSize(inst);
                    }
                }
            });
            inst.resizeObserver.observe(container);
        }

        _instances.set(containerId, inst);

        // Immediate first render blit if master canvas has content
        if (_masterRenderer && _scene && _camera) {
            _masterRenderer.render(_scene, _camera);
            if (canvas.width > 0 && canvas.height > 0) {
                ctx.clearRect(0, 0, canvas.width, canvas.height);
                ctx.drawImage(_masterRenderer.domElement, 0, 0, canvas.width, canvas.height);
            }
        }

        if (!_animId) {
            animate();
        }
    };

    window.disposeLinkDrop3d = function (containerId) {
        if (!_instances.has(containerId)) return;
        const inst = _instances.get(containerId);
        _instances.delete(containerId);

        if (inst.resizeObserver) {
            inst.resizeObserver.disconnect();
        }
        if (inst.cleanupListeners) {
            inst.cleanupListeners();
        }
        if (inst.canvas && inst.canvas.parentNode) {
            inst.canvas.parentNode.removeChild(inst.canvas);
        }

        if (_instances.size === 0 && _animId) {
            cancelAnimationFrame(_animId);
            _animId = null;
        }
    };
})();
