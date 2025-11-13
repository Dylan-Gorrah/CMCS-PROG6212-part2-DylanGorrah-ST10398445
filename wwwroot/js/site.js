// Cosmic UI Effects - Claims Management System

(function () {
    'use strict';

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        initCustomCursor();
        initParticles();
        initCardEffects();
        initSmoothScroll();
        initFormEnhancements();
    }

    // Custom Cursor System
    function initCustomCursor() {
        const cursor = document.querySelector('.cursor');
        const follower = document.querySelector('.cursor-follower');

        // Check if device supports hover (desktop)
        const isDesktop = window.matchMedia('(hover: hover) and (pointer: fine)').matches;
        const isLargeScreen = window.matchMedia('(min-width: 769px)').matches;

        if (!cursor || !follower || !isDesktop || !isLargeScreen) {
            return;
        }

        let mouseX = 0, mouseY = 0;
        let followerX = 0, followerY = 0;
        let isVisible = false;

        // Add cursor-active class to body
        document.body.classList.add('cursor-active');

        // Mouse move handler
        function handleMouseMove(e) {
            mouseX = e.clientX;
            mouseY = e.clientY;

            if (!isVisible) {
                cursor.classList.add('active');
                follower.classList.add('active');
                isVisible = true;
            }

            cursor.style.left = mouseX + 'px';
            cursor.style.top = mouseY + 'px';
        }

        // Smooth follower animation
        function animate() {
            const distX = mouseX - followerX;
            const distY = mouseY - followerY;

            followerX += distX * 0.1;
            followerY += distY * 0.1;

            follower.style.left = followerX + 'px';
            follower.style.top = followerY + 'px';

            requestAnimationFrame(animate);
        }

        // Mouse leave handler
        function handleMouseLeave() {
            cursor.classList.remove('active');
            follower.classList.remove('active');
            isVisible = false;
        }

        // Hover effect for interactive elements
        function addHoverEffect() {
            cursor.classList.add('hover');
        }

        function removeHoverEffect() {
            cursor.classList.remove('hover');
        }

        // Attach event listeners
        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseleave', handleMouseLeave);

        // Start animation loop immediately
        requestAnimationFrame(animate);

        // Add hover effects to interactive elements
        const hoverSelectors = [
            'a', 'button', '.btn', '.card', '.role-card',
            'input[type="submit"]', 'input[type="button"]',
            '.back-button', '.logout-button', 'tr'
        ];

        hoverSelectors.forEach(selector => {
            const elements = document.querySelectorAll(selector);
            elements.forEach(el => {
                el.addEventListener('mouseenter', addHoverEffect);
                el.addEventListener('mouseleave', removeHoverEffect);
            });
        });

        // Handle dynamic content (for SPA or lazy-loaded elements)
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1) { // Element node
                        hoverSelectors.forEach(selector => {
                            if (node.matches && node.matches(selector)) {
                                node.addEventListener('mouseenter', addHoverEffect);
                                node.addEventListener('mouseleave', removeHoverEffect);
                            }
                            const children = node.querySelectorAll ? node.querySelectorAll(selector) : [];
                            children.forEach(child => {
                                child.addEventListener('mouseenter', addHoverEffect);
                                child.addEventListener('mouseleave', removeHoverEffect);
                            });
                        });
                    }
                });
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    // Particle Background System
    function initParticles() {
        const particlesContainer = document.getElementById('particles');
        if (!particlesContainer) return;

        const particleCount = window.innerWidth < 768 ? 30 : 50;
        const fragment = document.createDocumentFragment();

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'particle';

            // Random positioning
            particle.style.left = Math.random() * 100 + '%';
            particle.style.top = Math.random() * 100 + '%';

            // Varied animation timing
            particle.style.animationDelay = Math.random() * 20 + 's';
            particle.style.animationDuration = (Math.random() * 10 + 15) + 's';

            // Varied sizes for depth
            const size = Math.random() * 2 + 1;
            particle.style.width = size + 'px';
            particle.style.height = size + 'px';

            fragment.appendChild(particle);
        }

        particlesContainer.appendChild(fragment);
    }

    // Card 3D Tilt Effects
    function initCardEffects() {
        const cards = document.querySelectorAll('.card, .role-card');
        if (!cards.length) return;

        cards.forEach(card => {
            let tiltTimeout;

            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;

                const centerX = rect.width / 2;
                const centerY = rect.height / 2;

                // Calculate rotation (reduced for subtlety)
                const rotateX = ((y - centerY) / centerY) * 8;
                const rotateY = ((centerX - x) / centerX) * 8;

                clearTimeout(tiltTimeout);
                card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-10px)`;
            });

            card.addEventListener('mouseleave', () => {
                // Smooth return to original position
                tiltTimeout = setTimeout(() => {
                    card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
                }, 50);
            });

            // Click effect
            card.addEventListener('click', function (e) {
                // Only add ripple if not clicking a button inside
                if (e.target.tagName === 'A' || e.target.tagName === 'BUTTON' ||
                    e.target.closest('a') || e.target.closest('button')) {
                    return;
                }

                const ripple = document.createElement('span');
                const rect = card.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;

                ripple.style.cssText = `
                    position: absolute;
                    width: ${size}px;
                    height: ${size}px;
                    left: ${x}px;
                    top: ${y}px;
                    background: rgba(249, 170, 173, 0.3);
                    border-radius: 50%;
                    transform: scale(0);
                    animation: ripple 0.6s ease-out;
                    pointer-events: none;
                `;

                card.style.position = 'relative';
                card.appendChild(ripple);

                setTimeout(() => ripple.remove(), 600);
            });
        });

        // Add ripple animation if not exists
        if (!document.querySelector('style[data-ripple]')) {
            const style = document.createElement('style');
            style.setAttribute('data-ripple', '');
            style.textContent = `
                @keyframes ripple {
                    to {
                        transform: scale(2);
                        opacity: 0;
                    }
                }
            `;
            document.head.appendChild(style);
        }
    }

    // Smooth Scroll for Anchor Links
    function initSmoothScroll() {
        const anchorLinks = document.querySelectorAll('a[href^="#"]');

        anchorLinks.forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#' || href === '#!') return;

                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Form Enhancements
    function initFormEnhancements() {
        // Auto-resize textareas
        const textareas = document.querySelectorAll('textarea.form-control');
        textareas.forEach(textarea => {
            textarea.addEventListener('input', function () {
                this.style.height = 'auto';
                this.style.height = (this.scrollHeight) + 'px';
            });
        });

        // Add floating label effect
        const formControls = document.querySelectorAll('.form-control');
        formControls.forEach(input => {
            // Check if already has value on load
            if (input.value) {
                input.classList.add('has-value');
            }

            input.addEventListener('blur', function () {
                if (this.value) {
                    this.classList.add('has-value');
                } else {
                    this.classList.remove('has-value');
                }
            });
        });

        // Form submission animation
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function (e) {
                const submitBtn = this.querySelector('button[type="submit"], input[type="submit"]');
                if (submitBtn && !submitBtn.disabled) {
                    submitBtn.style.opacity = '0.6';
                    submitBtn.style.pointerEvents = 'none';
                }
            });
        });
    }

    // Utility: Throttle function
    function throttle(func, wait) {
        let timeout;
        let lastRan;
        return function executedFunction(...args) {
            if (!lastRan) {
                func.apply(this, args);
                lastRan = Date.now();
            } else {
                clearTimeout(timeout);
                timeout = setTimeout(function () {
                    if ((Date.now() - lastRan) >= wait) {
                        func.apply(this, args);
                        lastRan = Date.now();
                    }
                }, wait - (Date.now() - lastRan));
            }
        };
    }

    // Handle resize events
    window.addEventListener('resize', throttle(() => {
        // Reinit cursor if needed
        const isDesktop = window.matchMedia('(hover: hover) and (pointer: fine)').matches;
        const isLargeScreen = window.matchMedia('(min-width: 769px)').matches;

        if (!isDesktop || !isLargeScreen) {
            const cursor = document.querySelector('.cursor');
            const follower = document.querySelector('.cursor-follower');
            if (cursor) cursor.classList.remove('active');
            if (follower) follower.classList.remove('active');
            document.body.classList.remove('cursor-active');
        } else {
            document.body.classList.add('cursor-active');
        }
    }, 250));

})();