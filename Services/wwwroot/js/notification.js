/* WangenPizza admin — Stand: 2026-04-05 (SSE|LongPolling, withCredentials, HTTP2-IIS) */
$(function () {
    var count = 0;
    var audioContext = null;
    var orderRingInterval = null;
    var activeOrderAudio = null;

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
            var duration = (durationMs || 220) / 1000;

            osc.type = "sine";
            osc.frequency.setValueAtTime(frequency || 950, now);

            gain.gain.setValueAtTime(0.001, now);
            gain.gain.exponentialRampToValueAtTime(volume || 0.25, now + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, now + duration);

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(now);
            osc.stop(now + duration + 0.01);
        } catch (e) {
            console.error("Beep failed:", e);
        }
    }

    function playNotificationSound() {
        var orderAudio = document.getElementById("orderNotificationSound");
        if (orderAudio) {
            orderAudio.loop = false;
            orderAudio.currentTime = 0;
            orderAudio.play().catch(function (err) {
                console.warn("orderNotificationSound play() abgelehnt oder fehlgeschlagen:", err && err.message ? err.message : err);
                // Fallback if file is blocked/missing
                beep(220, 920, 0.32);
                setTimeout(function () {
                    beep(220, 1020, 0.28);
                }, 180);
            });
            return;
        }

        // Fallback tone
        beep(220, 920, 0.32);
        setTimeout(function () {
            beep(220, 1020, 0.28);
        }, 180);
    }

    function startOrderRinging() {
        if (orderRingInterval) {
            clearInterval(orderRingInterval);
            orderRingInterval = null;
        }

        var orderAudio = document.getElementById("orderNotificationSound");
        if (orderAudio) {
            stopAllSounds();
            orderAudio.loop = true;
            orderAudio.currentTime = 0;
            orderAudio.play().then(function () {
                activeOrderAudio = orderAudio;
            }).catch(function () {
                // If mp3 cannot play, fallback to periodic beeps.
                playNotificationSound();
                orderRingInterval = setInterval(function () {
                    playNotificationSound();
                }, 1800);
            });
            return;
        }

        playNotificationSound();
        orderRingInterval = setInterval(function () {
            playNotificationSound();
        }, 1800);
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
        // SSE (2) | LongPolling (4) = 6 — ohne WebSocket. Reine LP auf HTTP/2 löst oft net::ERR_HTTP2_PROTOCOL_ERROR auf IIS.
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
                toastr.success(message, null, { toastClass: "toast-order" });
                startOrderRinging();
                return;
            } else if (notificationType === "cash") {
                updateCount(++count);
                toastr.success(message, null, { toastClass: "toast-default" });
                startOrderRinging();
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
        }).catch(function (err) {
            console.error("SignalR connection failed:", err.toString());
        });
    } else {
        console.warn("SignalR not loaded; notification sound test button still active.");
    }

    // Unlock audio engine on first user interaction
    $(document).one("click keydown touchstart", function () {
        var ctx = ensureAudioContext();
        if (ctx && ctx.state === "suspended") {
            ctx.resume().catch(function () { });
        }
    });

    // Test sound button
    $(document).on("click", "#notifyButton", function () {
        stopAllSounds();
        playNotificationSound();
    });

    // Andere Buttons stoppen nur Klingeln — nicht #notifyButton (target kann Kind-Element sein, daher closest).
    $(document).on("click", "button", function (e) {
        if ($(e.target).closest("#notifyButton").length) {
            return;
        }
        stopAllSounds();
    });

    $("#notificationCount").on("click", function () {
        updateCount(0);
        sessionStorage.setItem("originalNotificationCount", count);
    });
});
