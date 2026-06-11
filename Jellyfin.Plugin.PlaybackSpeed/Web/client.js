(function () {
    "use strict";

    var config = window.JellyfinPlaybackSpeedConfig || {};
    if (config.Enabled === false) {
        return;
    }

    var storageKey = "jellyfin-playback-speed";
    var controlId = "playback-speed-plugin-control";
    var speeds = Array.isArray(config.Speeds) && config.Speeds.length ? config.Speeds : [1, 1.25, 1.5, 2];
    var speed = readSpeed();
    var activeMedia = null;

    function readSpeed() {
        var stored = Number(window.localStorage.getItem(storageKey));
        if (Number.isFinite(stored) && stored > 0) {
            return stored;
        }

        var defaultSpeed = Number(config.DefaultSpeed);
        return Number.isFinite(defaultSpeed) && defaultSpeed > 0 ? defaultSpeed : 1;
    }

    function saveSpeed(nextSpeed) {
        speed = nextSpeed;
        window.localStorage.setItem(storageKey, String(nextSpeed));
    }

    function findMedia() {
        var media = Array.prototype.slice.call(document.querySelectorAll("audio, video"));
        return media.find(function (element) { return !element.paused; }) || media[0] || null;
    }

    function applySpeed(media) {
        if (!media || media.playbackRate === speed) {
            return;
        }

        media.playbackRate = speed;
    }

    function syncMedia() {
        activeMedia = findMedia();
        applySpeed(activeMedia);
        updateButton();
    }

    function closestSpeedIndex() {
        return speeds.reduce(function (closest, value, index) {
            var currentDelta = Math.abs(speeds[closest] - speed);
            var nextDelta = Math.abs(value - speed);
            return nextDelta < currentDelta ? index : closest;
        }, 0);
    }

    function cycleSpeed(direction) {
        var nextIndex = Math.max(0, Math.min(speeds.length - 1, closestSpeedIndex() + direction));
        setSpeed(speeds[nextIndex]);
    }

    function setSpeed(nextSpeed) {
        saveSpeed(nextSpeed);
        applySpeed(activeMedia || findMedia());
        updateButton();
    }

    function updateButton() {
        var label = document.querySelector("#" + controlId + " .jps-current");
        if (label) {
            label.textContent = speed + "x";
        }
    }

    function createControl() {
        if (config.ShowFloatingControl === false || document.getElementById(controlId)) {
            return;
        }

        var root = document.createElement("div");
        root.id = controlId;
        root.innerHTML = [
            '<button type="button" class="jps-step" data-step="-1" aria-label="Decrease playback speed">-</button>',
            '<button type="button" class="jps-current" aria-label="Choose playback speed">' + speed + "x</button>",
            '<button type="button" class="jps-step" data-step="1" aria-label="Increase playback speed">+</button>',
            '<div class="jps-menu" role="menu" aria-label="Playback speed">',
            speeds.map(function (value) {
                return '<button type="button" role="menuitem" data-speed="' + value + '">' + value + "x</button>";
            }).join(""),
            "</div>"
        ].join("");

        root.addEventListener("click", function (event) {
            var target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            if (target.dataset.step) {
                cycleSpeed(Number(target.dataset.step));
                return;
            }

            if (target.dataset.speed) {
                setSpeed(Number(target.dataset.speed));
                root.classList.remove("jps-open");
                return;
            }

            if (target.classList.contains("jps-current")) {
                root.classList.toggle("jps-open");
            }
        });

        document.documentElement.append(root);
    }

    function createStyle() {
        if (document.getElementById(controlId + "-style")) {
            return;
        }

        var style = document.createElement("style");
        style.id = controlId + "-style";
        style.textContent = [
            "#" + controlId + "{position:fixed;right:18px;bottom:86px;z-index:100000;display:flex;align-items:center;gap:6px;font:600 14px/1.2 system-ui,-apple-system,BlinkMacSystemFont,\"Segoe UI\",sans-serif}",
            "#" + controlId + " button{min-width:38px;height:34px;border:0;border-radius:6px;color:#fff;background:rgba(24,26,32,.92);box-shadow:0 2px 10px rgba(0,0,0,.35);cursor:pointer}",
            "#" + controlId + " button:hover,#" + controlId + " button:focus-visible{background:#00a4dc;outline:none}",
            "#" + controlId + " .jps-current{min-width:58px}",
            "#" + controlId + " .jps-menu{position:absolute;right:0;bottom:42px;display:none;grid-template-columns:repeat(2,minmax(58px,1fr));gap:6px;padding:8px;border-radius:8px;background:rgba(10,12,16,.96);box-shadow:0 8px 24px rgba(0,0,0,.45)}",
            "#" + controlId + ".jps-open .jps-menu{display:grid}"
        ].join("\n");
        document.head.append(style);
    }

    document.addEventListener("play", syncMedia, true);
    document.addEventListener("ratechange", function () {
        var media = findMedia();
        if (media && media.playbackRate !== speed) {
            applySpeed(media);
        }
    }, true);

    var observer = new MutationObserver(function () {
        createStyle();
        createControl();
        syncMedia();
    });

    createStyle();
    createControl();
    syncMedia();
    observer.observe(document.documentElement, { childList: true, subtree: true });
    window.setInterval(syncMedia, 2000);
}());
