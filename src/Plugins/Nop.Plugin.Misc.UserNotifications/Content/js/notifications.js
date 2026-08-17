/**
 * User Notifications Module - Interactive Client Script
 */

(function () {
    'use strict';

    window.UserNotifications = {
        config: {
            pollInterval: 35000,
            soundEnabled: true,
            pollUrl: '/customer/notifications/poll',
            flyoutUrl: '/customer/notifications/flyout-items',
            markReadUrl: '/customer/notifications/mark-read',
            markAllReadUrl: '/customer/notifications/mark-all-read',
            dismissPopupUrl: '/customer/notifications/dismiss-popup'
        },

        init: function () {
            this.bindFlyoutEvents();
            this.bindActionEvents();
            this.startHeartbeat();
        },

        // 1. Audio Synthesizer Chime (Web Audio API)
        playChime: function () {
            if (!this.config.soundEnabled) return;
            try {
                var AudioContext = window.AudioContext || window.webkitAudioContext;
                if (!AudioContext) return;
                var ctx = new AudioContext();

                var osc = ctx.createOscillator();
                var gain = ctx.createGain();

                osc.type = 'sine';
                osc.frequency.setValueAtTime(587.33, ctx.currentTime); // D5
                osc.frequency.exponentialRampToValueAtTime(880, ctx.currentTime + 0.15); // A5

                gain.gain.setValueAtTime(0.05, ctx.currentTime);
                gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);

                osc.connect(gain);
                gain.connect(ctx.destination);

                osc.start();
                osc.stop(ctx.currentTime + 0.4);
            } catch (e) {
                // AudioContext might be blocked until user interaction
            }
        },

        // 2. Confetti Particle Canvas Burst
        fireConfetti: function () {
            try {
                var canvas = document.getElementById('notif-confetti-canvas');
                if (!canvas) {
                    canvas = document.createElement('canvas');
                    canvas.id = 'notif-confetti-canvas';
                    canvas.style.position = 'fixed';
                    canvas.style.top = '0';
                    canvas.style.left = '0';
                    canvas.style.width = '100vw';
                    canvas.style.height = '100vh';
                    canvas.style.pointerEvents = 'none';
                    canvas.style.zIndex = '10700';
                    document.body.appendChild(canvas);
                }

                var ctx = canvas.getContext('2d');
                canvas.width = window.innerWidth;
                canvas.height = window.innerHeight;

                var particles = [];
                var colors = ['#4f46e5', '#f59e0b', '#10b981', '#ec4899', '#3b82f6', '#8b5cf6'];

                for (var i = 0; i < 70; i++) {
                    particles.push({
                        x: canvas.width / 2,
                        y: canvas.height / 2,
                        r: Math.random() * 6 + 4,
                        dx: (Math.random() - 0.5) * 16,
                        dy: (Math.random() - 0.7) * 16,
                        color: colors[Math.floor(Math.random() * colors.length)],
                        tilt: Math.random() * 10,
                        tiltAngle: 0,
                        tiltAngleInc: Math.random() * 0.1 + 0.05,
                        life: 1
                    });
                }

                function render() {
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                    var active = false;

                    for (var i = 0; i < particles.length; i++) {
                        var p = particles[i];
                        if (p.life > 0.01) {
                            active = true;
                            p.x += p.dx;
                            p.y += p.dy;
                            p.dy += 0.35; // gravity
                            p.life *= 0.96;
                            p.tiltAngle += p.tiltAngleInc;

                            ctx.beginPath();
                            ctx.fillStyle = p.color;
                            ctx.globalAlpha = p.life;
                            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                            ctx.fill();
                        }
                    }

                    if (active) {
                        requestAnimationFrame(render);
                    } else {
                        ctx.clearRect(0, 0, canvas.width, canvas.height);
                        if (canvas.parentNode) canvas.parentNode.removeChild(canvas);
                    }
                }
                render();
            } catch (err) { }
        },

        // 3. Header Bell Flyout Dropdown
        bindFlyoutEvents: function () {
            var self = this;
            var bellBtn = document.getElementById('notif-bell-toggle');
            var dropdown = document.getElementById('notif-flyout-menu');

            if (!bellBtn || !dropdown) return;

            bellBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                dropdown.classList.toggle('show');
            });

            document.addEventListener('click', function (e) {
                if (!dropdown.contains(e.target) && !bellBtn.contains(e.target)) {
                    dropdown.classList.remove('show');
                }
            });

            // Flyout Category Tabs
            var tabs = dropdown.querySelectorAll('.notif-flyout-tab');
            tabs.forEach(function (tab) {
                tab.addEventListener('click', function (e) {
                    e.preventDefault();
                    tabs.forEach(function (t) { t.classList.remove('active'); });
                    tab.classList.add('active');
                    var category = tab.getAttribute('data-category') || 'All';
                    self.loadFlyoutItems(category);
                });
            });

            // Mark all read button in flyout
            var markAllBtn = dropdown.querySelector('.notif-mark-all-btn');
            if (markAllBtn) {
                markAllBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    self.markAllAsRead();
                });
            }
        },

        loadFlyoutItems: function (category) {
            var listEl = document.getElementById('notif-flyout-list');
            if (!listEl) return;

            fetch(this.config.flyoutUrl + '?category=' + encodeURIComponent(category))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (!data || !data.success) return;

                    if (data.items && data.items.length > 0) {
                        var html = '';
                        data.items.forEach(function (item) {
                            html += '<li class="notif-item-card ' + (item.isRead ? '' : 'unread') + '" data-id="' + item.id + '">';
                            html += '  <div class="notif-item-icon category-' + item.category + '"><i class="fas ' + item.icon + '"></i></div>';
                            html += '  <div class="notif-item-content">';
                            html += '    <h4 class="notif-item-title">' + item.title + '</h4>';
                            html += '    <p class="notif-item-message">' + item.message + '</p>';
                            html += '    <div class="notif-item-meta">';
                            html += '      <span>' + item.relativeTime + '</span>';
                            if (item.couponCode) {
                                html += '      <span class="notif-coupon-tag" onclick="UserNotifications.copyCoupon(\'' + item.couponCode + '\', event)"><i class="fas fa-ticket-alt"></i> ' + item.couponCode + '</span>';
                            }
                            html += '    </div>';
                            html += '  </div>';
                            html += '</li>';
                        });
                        listEl.innerHTML = html;
                    } else {
                        listEl.innerHTML = '<li class="notif-empty-state"><div class="notif-empty-icon"><i class="far fa-bell-slash"></i></div><div class="notif-empty-title">No notifications</div><p class="notif-empty-desc">You are all caught up!</p></li>';
                    }
                });
        },

        // 4. Mark Read & Actions
        markAsRead: function (id, cardEl) {
            var self = this;
            fetch(this.config.markReadUrl + '?id=' + id, { method: 'POST' })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (cardEl) {
                        cardEl.classList.remove('unread');
                    }
                    if (data && typeof data.unreadCount !== 'undefined') {
                        self.updateBadge(data.unreadCount);
                    }
                });
        },

        markAllAsRead: function () {
            var self = this;
            fetch(this.config.markAllReadUrl, { method: 'POST' })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    self.updateBadge(0);
                    var cards = document.querySelectorAll('.notif-item-card, .notif-hub-card');
                    cards.forEach(function (c) { c.classList.remove('unread'); });
                });
        },

        updateBadge: function (count) {
            var badges = document.querySelectorAll('.notif-badge-pill, .notif-unread-count');
            badges.forEach(function (b) {
                b.textContent = count;
                b.style.display = count > 0 ? 'inline-block' : 'none';
            });
        },

        copyCoupon: function (code, e) {
            if (e) {
                e.preventDefault();
                e.stopPropagation();
            }
            if (!code) return;

            navigator.clipboard.writeText(code).then(function () {
                UserNotifications.showToast({
                    title: 'Coupon Copied! 🎉',
                    message: 'Code ' + code + ' copied to clipboard. Apply at checkout for discount!',
                    icon: 'fa-check-circle',
                    category: 'Promotion'
                });
            }).catch(function () {
                prompt('Copy coupon code:', code);
            });
        },

        // 5. Toast Stack Engine
        showToast: function (options) {
            var stack = document.getElementById('notif-toast-stack');
            if (!stack) {
                stack = document.createElement('div');
                stack.id = 'notif-toast-stack';
                stack.className = 'notif-toast-stack';
                document.body.appendChild(stack);
            }

            var toast = document.createElement('div');
            toast.className = 'notif-toast-card';

            var iconHtml = options.imageUrl
                ? '<img src="' + options.imageUrl + '" class="notif-toast-thumb" alt="Product" />'
                : '<div class="notif-item-icon category-' + (options.category || 'Promotion') + '"><i class="fas ' + (options.icon || 'fa-bell') + '"></i></div>';

            var couponHtml = options.couponCode
                ? '<div style="margin-top: 6px;"><span class="notif-coupon-tag" onclick="UserNotifications.copyCoupon(\'' + options.couponCode + '\', event)"><i class="fas fa-ticket-alt"></i> ' + options.couponCode + ' (Copy)</span></div>'
                : '';

            var actionBtnHtml = options.actionUrl
                ? '<a href="' + options.actionUrl + '" class="btn btn-sm btn-outline-primary" style="margin-top: 6px; font-size: 0.75rem; padding: 2px 8px;">View Details</a>'
                : '';

            toast.innerHTML = iconHtml +
                '<div style="flex-grow: 1; min-width: 0;">' +
                '  <h5 class="notif-item-title">' + options.title + '</h5>' +
                '  <p class="notif-item-message" style="margin-bottom: 2px;">' + options.message + '</p>' +
                couponHtml + actionBtnHtml +
                '</div>' +
                '<button type="button" class="notif-toast-close">&times;</button>' +
                '<div class="notif-toast-progress"></div>';

            stack.appendChild(toast);
            this.playChime();

            var closeBtn = toast.querySelector('.notif-toast-close');
            var timeoutId = setTimeout(dismiss, 6000);

            function dismiss() {
                clearTimeout(timeoutId);
                toast.classList.add('hide');
                setTimeout(function () {
                    if (toast.parentNode) toast.parentNode.removeChild(toast);
                }, 300);
                if (options.id) {
                    fetch('/customer/notifications/dismiss-popup?id=' + options.id, { method: 'POST' });
                }
            }

            closeBtn.addEventListener('click', dismiss);
        },

        // 6. Action Bindings for Page Elements
        bindActionEvents: function () {
            var self = this;

            // Copy buttons
            document.querySelectorAll('[data-copy-coupon]').forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    var code = btn.getAttribute('data-copy-coupon');
                    self.copyCoupon(code, e);
                });
            });
        },

        // 7. Background Heartbeat Polling Loop
        startHeartbeat: function () {
            var self = this;
            if (!document.getElementById('notif-bell-toggle')) return;

            setInterval(function () {
                fetch(self.config.pollUrl)
                    .then(function (res) { return res.json(); })
                    .then(function (data) {
                        if (!data || !data.success) return;

                        if (typeof data.unreadCount !== 'undefined') {
                            self.updateBadge(data.unreadCount);
                        }

                        if (data.popups && data.popups.length > 0) {
                            data.popups.forEach(function (popup) {
                                if (popup.popupType === 'Celebration') {
                                    self.fireConfetti();
                                }
                                self.showToast(popup);
                            });
                        }
                    }).catch(function () { });
            }, this.config.pollInterval);
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        window.UserNotifications.init();
    });
})();
