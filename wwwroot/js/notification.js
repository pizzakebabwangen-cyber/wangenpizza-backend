/* WangenPizza admin — Stand: 2026-06-15 (lauter, Dauerton bis Akzeptieren) */
/* Use jQuery( fn ) so $ inside callback is correct after layout calls jQuery.noConflict() */
jQuery(function ($) {
    var count = 0;
    var audioContext = null;
    var orderRingInterval = null;
    var activeOrderAudio = null;
    var pendingRingWatchInterval = null;
    var posPendingPollInterval = null;
    var soundUnlocked = localStorage.getItem("wangenAdminSoundUnlocked") === "1";
    var sessionAudioUnlocked = sessionStorage.getItem("wangenAudioSessionUnlocked") === "1";
    var lastKnownPendingOrderId = parseInt(localStorage.getItem("wangenAdminLastPendingOrderId") || "0", 10) || 0;

    var MAX_AUDIO_VOLUME = 1.0;
    var FALLBACK_RING_MS = 1400;
    var POS_POLL_MS = 12000;
    var PENDING_WATCH_MS = 700;

    function markSoundUnlocked() {
        soundUnlocked = true;
        sessionAudioUnlocked = true;
        localStorage.setItem("wangenAdminSoundUnlocked", "1");
        sessionStorage.setItem("wangenAudioSessionUnlocked", "1");
    }

    function isAudioReadyForAutoplay() {
        return sessionAudioUnlocked || soundUnlocked;
    }

    function showSoundActivationHint() {
        if (window.toastr) {
            toastr.warning(
                "Einmal irgendwo auf die Seite klicken — danach kommt der Bestellton automatisch.",
                null,
                { timeOut: 8000 }
            );
        }
    }

    function requestDesktopNotificationPermission() {
        if (!("Notification" in window)) return;
        if (Notification.permission === "granted" || Notification.permission === "denied") return;
        try {
            Notification.requestPermission();
        } catch (e) { }
    }

    function showDesktopOrderAlert(message) {
        if (!("Notification" in window) || Notification.permission !== "granted") return;
        try {
            new Notification("Neue Bestellung — Pizza Wangen", {
                body: message || "Neue Bestellung im POS",
                silent: false,
                requireInteraction: true,
                tag: "wangen-pos-order"
            });
        } catch (e) {
            console.warn("Desktop notification failed:", e);
        }
    }

    function configureOrderAudio(audioEl) {
        if (!audioEl) return;
        audioEl.volume = MAX_AUDIO_VOLUME;
        audioEl.muted = false;
        audioEl.preload = "auto";
    }

    function ensureAudioContext() {
        if (audioContext) {
            return audioContext;
        }

        var AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) {
            return null;
        }

        audioContext = new AudioCtx();
        return audioContext;
    }

    function unlockHtml5Audio() {
        var orderAudio = document.getElementById("orderNotificationSound");
        if (!orderAudio) return;

        configureOrderAudio(orderAudio);
        orderAudio.loop = false;
        var p = orderAudio.play();
        if (p && typeof p.then === "function") {
            p.then(function () {
                orderAudio.pause();
                orderAudio.currentTime = 0;
                markSoundUnlocked();
            }).catch(function () { });
        }
    }

    function beep(durationMs, frequency, volume) {
        try {
            var ctx = ensureAudioContext();
            if (!ctx) return;

            if (ctx.state === "suspended") {
                ctx.resume();
            }

            var osc = ctx.createOscillator();
            var gain = ctx.createGain();
            var now = ctx.currentTime;
            var duration = (durationMs || 480) / 1000;

            osc.type = "square";
            osc.frequency.setValueAtTime(frequency || 880, now);

            var peak = volume || 0.78;
            gain.gain.setValueAtTime(0.001, now);
            gain.gain.exponentialRampToValueAtTime(peak, now + 0.015);
            gain.gain.exponentialRampToValueAtTime(0.001, now + duration);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(now);
            osc.stop(now + duration + 0.02);
        } catch (e) {
            console.error("Beep failed:", e);
        }
    }

    /** Lauter Alarm-Fallback wenn MP3 blockiert ist — drei deutliche Töne. */
    function playAlarmFallback() {
        beep(520, 820, 0.82);
        setTimeout(function () { beep(520, 980, 0.82); }, 520);
        setTimeout(function () { beep(620, 880, 0.88); }, 1040);
    }

    function startFallbackRinging() {
        if (orderRingInterval) {
            clearInterval(orderRingInterval);
        }
        playAlarmFallback();
        orderRingInterval = setInterval(playAlarmFallback, FALLBACK_RING_MS);
    }

    function playNotificationSound() {
        var orderAudio = document.getElementById("orderNotificationSound");
        if (orderAudio) {
            configureOrderAudio(orderAudio);
            orderAudio.loop = false;
            orderAudio.pause();
            orderAudio.currentTime = 0;
            orderAudio.play().then(function () {
                markSoundUnlocked();
            }).catch(function (err) {
                console.warn("orderNotificationSound play() abgelehnt oder fehlgeschlagen:", err && err.message ? err.message : err);
                if (!soundUnlocked) {
                    showSoundActivationHint();
                }
                playAlarmFallback();
            });
            return;
        }

        playAlarmFallback();
    }

    function notifyNewOrder(message) {
        startOrderRinging();
        showDesktopOrderAlert(message);
    }

    function startOrderRinging() {
        if (!isAudioReadyForAutoplay()) {
            showSoundActivationHint();
        }

        if (orderRingInterval) {
            clearInterval(orderRingInterval);
            orderRingInterval = null;
        }

        var orderAudio = document.getElementById("orderNotificationSound");
        if (orderAudio) {
            stopAllSounds();
            configureOrderAudio(orderAudio);
            orderAudio.loop = true;
            orderAudio.currentTime = 0;
            orderAudio.play().then(function () {
                markSoundUnlocked();
                activeOrderAudio = orderAudio;
            }).catch(function () {
                if (!soundUnlocked) {
                    showSoundActivationHint();
                }
                startFallbackRinging();
            });
            return;
        }

        startFallbackRinging();
    }

    function stopAllSounds() {
        if (orderRingInterval) {
            clearInterval(orderRingInterval);
            orderRingInterval = null;
        }

        if (activeOrderAudio) {
            activeOrderAudio.loop = false;
            activeOrderAudio.pause();
            activeOrderAudio.currentTime = 0;
            activeOrderAudio = null;
        }

        var orderAudio = document.getElementById("orderNotificationSound");
        if (orderAudio) {
            orderAudio.loop = false;
            orderAudio.pause();
            orderAudio.currentTime = 0;
        }
        var alertAudio = document.getElementById("alertNotificationSound");
        if (alertAudio) {
            alertAudio.loop = false;
            alertAudio.pause();
            alertAudio.currentTime = 0;
        }
    }

    function updateCount(newCount) {
        count = newCount;
        $("#notificationCount").text(count);
    }

    function hasPendingUnacceptedOrders() {
        if (typeof window.wangenHasPendingUnacceptedOrders === "boolean") {
            localStorage.setItem("wangenAdminPendingUnacceptedOrders", window.wangenHasPendingUnacceptedOrders ? "1" : "0");
            return window.wangenHasPendingUnacceptedOrders;
        }

        return localStorage.getItem("wangenAdminPendingUnacceptedOrders") === "1";
    }

    function markPendingUnacceptedOrders() {
        localStorage.setItem("wangenAdminPendingUnacceptedOrders", "1");
    }

    function clearPendingUnacceptedOrders() {
        window.wangenHasPendingUnacceptedOrders = false;
        localStorage.setItem("wangenAdminPendingUnacceptedOrders", "0");
        stopAllSounds();
    }

    function keepRingingIfPending() {
        if (!hasPendingUnacceptedOrders()) {
            return;
        }

        setTimeout(function () {
            ensurePendingOrderRinging();
        }, 150);
    }

    function isOrderRingingActive() {
        if (orderRingInterval) {
            return true;
        }

        return !!(activeOrderAudio && activeOrderAudio.loop && !activeOrderAudio.paused);
    }

    function ensurePendingOrderRinging() {
        if (!hasPendingUnacceptedOrders()) {
            stopAllSounds();
            return;
        }

        if (!isOrderRingingActive()) {
            startOrderRinging();
            return;
        }

        // MP3 kann im Hintergrund pausieren — bei Bedarf neu starten.
        if (activeOrderAudio && activeOrderAudio.paused) {
            startOrderRinging();
        }
    }

    function startPendingRingWatch() {
        if (pendingRingWatchInterval) {
            clearInterval(pendingRingWatchInterval);
        }

        pendingRingWatchInterval = setInterval(ensurePendingOrderRinging, PENDING_WATCH_MS);
    }

    function checkPosPendingOrders() {
        $.ajax({
            url: "/Order/PosPendingStatus",
            method: "GET",
            cache: false,
            timeout: 10000
        }).done(function (data) {
            var hasPending = !!(data && data.hasPendingUnacceptedOrders);
            var latestId = parseInt((data && data.latestOrderId) || "0", 10) || 0;
            if (!hasPending) {
                clearPendingUnacceptedOrders();
                return;
            }

            window.wangenHasPendingUnacceptedOrders = true;
            markPendingUnacceptedOrders();

            if (latestId > lastKnownPendingOrderId) {
                lastKnownPendingOrderId = latestId;
                localStorage.setItem("wangenAdminLastPendingOrderId", String(latestId));
                notifyNewOrder("Neue Bestellung #" + latestId);
                return;
            }

            ensurePendingOrderRinging();
        }).fail(function (xhr, status) {
            if (status !== "abort") {
                console.warn("POS pending check failed:", status);
            }
        });
    }

    function startPosPendingPoll() {
        if (posPendingPollInterval) {
            clearInterval(posPendingPollInterval);
        }
        setTimeout(checkPosPendingOrders, 800);
        posPendingPollInterval = setInterval(checkPosPendingOrders, POS_POLL_MS);
    }

    var originalCount = parseInt(sessionStorage.getItem("originalNotificationCount")) || 0;
    updateCount(originalCount);

    toastr.options = {
        closeButton: true,
        positionClass: "toast-bottom-right",
        timeOut: "0",
        extendedTimeOut: 0,
        tapToDismiss: false
    };

    if (window.signalR && window.signalR.HubConnectionBuilder) {
        var httpT = signalR.HttpTransportType;
        var sseLp = (httpT && httpT.ServerSentEvents !== undefined && httpT.LongPolling !== undefined)
            ? (httpT.ServerSentEvents | httpT.LongPolling)
            : 6;
        var connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub", { transport: sseLp, withCredentials: true })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 20000, 40000])
            .build();

        connection.onreconnecting(function (err) {
            console.warn("SignalR: Reconnecting…", err ? err.message : "");
        });
        connection.onreconnected(function () {
            console.log("SignalR: wieder verbunden");
            checkPosPendingOrders();
        });
        connection.onclose(function (err) {
            if (err) {
                console.warn("SignalR: Verbindung getrennt (kein Auto-Reconnect mehr):", err.message || err);
            }
        });

        connection.on("ReceiveNotification", function (message, notificationType) {
            console.log("ReceiveNotification:", notificationType, message);
            if (notificationType === "order") {
                updateCount(++count);
                markPendingUnacceptedOrders();
                toastr.success(message, null, { toastClass: "toast-order" });
                notifyNewOrder(message);
                return;
            } else if (notificationType === "cash") {
                updateCount(++count);
                markPendingUnacceptedOrders();
                toastr.success(message, null, { toastClass: "toast-default" });
                notifyNewOrder(message);
                return;
            } else if (notificationType === "alert") {
                toastr.success(message, null, { toastClass: "toast-alert" });
            } else {
                toastr.success(message, null, { toastClass: "toast-default" });
            }

            playNotificationSound();
        });

        connection.start().then(function () {
            console.log("SignalR connected (SSE|LongPolling, no WebSocket)");
            checkPosPendingOrders();
        }).catch(function (err) {
            console.error("SignalR connection failed:", err.toString());
        });
    } else {
        console.warn("SignalR not loaded; notification sound test button still active.");
    }

    if (hasPendingUnacceptedOrders()) {
        setTimeout(function () {
            ensurePendingOrderRinging();
        }, 400);
    }
    startPendingRingWatch();
    startPosPendingPoll();

    window.addEventListener("pageshow", keepRingingIfPending);
    window.addEventListener("focus", function () {
        checkPosPendingOrders();
        keepRingingIfPending();
    });

    function unlockAllAudio() {
        var ctx = ensureAudioContext();
        if (ctx && ctx.state === "suspended") {
            ctx.resume().then(markSoundUnlocked).catch(function () { });
        } else {
            markSoundUnlocked();
        }
        unlockHtml5Audio();
        requestDesktopNotificationPermission();
    }

    // Einmal irgendwo klicken/tippen reicht — «Ton abspielen» ist optional.
    $(document).on("click keydown touchstart", function (e) {
        if (!sessionAudioUnlocked) {
            unlockAllAudio();
        }
    });

    if (!sessionAudioUnlocked) {
        setTimeout(showSoundActivationHint, 1500);
    }

    // Ton abspielen: freischalten — bei offener Bestellung Dauerton neu starten (nicht stoppen).
    $(document).on("click", "#notifyButton", function () {
        unlockAllAudio();
        if (hasPendingUnacceptedOrders()) {
            startOrderRinging();
            return;
        }
        stopAllSounds();
        playNotificationSound();
    });

    $(document).on("click keydown touchstart", function (e) {
        if ($(e.target).closest("#notifyButton").length) {
            return;
        }
        if (hasPendingUnacceptedOrders()) {
            keepRingingIfPending();
        }
    });

    $("#notificationCount").on("click", function () {
        updateCount(0);
        sessionStorage.setItem("originalNotificationCount", count);
    });
});
