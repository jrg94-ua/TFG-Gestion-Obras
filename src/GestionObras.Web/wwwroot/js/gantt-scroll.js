window.gestionObrasGanttScroll = {
    init: function (topId, containerId) {
        const top = document.getElementById(topId);
        const container = document.getElementById(containerId);

        if (!top || !container) {
            return;
        }

        if (top.dataset.syncBound === "true") {
            return;
        }

        let syncingTop = false;
        let syncingContainer = false;

        top.addEventListener("scroll", function () {
            if (syncingContainer) {
                return;
            }

            syncingTop = true;
            container.scrollLeft = top.scrollLeft;
            syncingTop = false;
        });

        container.addEventListener("scroll", function () {
            if (syncingTop) {
                return;
            }

            syncingContainer = true;
            top.scrollLeft = container.scrollLeft;
            syncingContainer = false;
        });

        top.dataset.syncBound = "true";
        top.scrollLeft = container.scrollLeft;
    }
};