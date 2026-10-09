(() => {
    "use strict";

    const storageKey = "cv910-ai-design";
    const savedPreference = window.localStorage.getItem(storageKey);
    let enabled = savedPreference === null ? true : savedPreference === "on";

    const paperBackground = document.createElement("div");
    paperBackground.id = "ai-paper-background";
    paperBackground.setAttribute("aria-hidden", "true");

    for (let index = 0; index < 4; index += 1) {
        const paper = document.createElement("span");
        paper.className = "ai-paper-sheet";
        paperBackground.append(paper);
    }

    document.body.prepend(paperBackground);

    const toggle = document.createElement("button");
    toggle.type = "button";
    toggle.className = "ai-background-toggle";
    toggle.setAttribute("aria-label", "Zapnúť alebo vypnúť celý AI dizajn");
    toggle.innerHTML = `
    <span class="ai-toggle-copy">
      <span class="ai-toggle-eyebrow">AI DEMO</span>
      <span class="ai-toggle-label">AI dizajn</span>
      <span class="ai-toggle-status" aria-hidden="true"></span>
    </span>
    <span class="ai-toggle-track" aria-hidden="true">
      <span class="ai-toggle-thumb"></span>
    </span>`;

    document.body.append(toggle);
    const status = toggle.querySelector(".ai-toggle-status");

    function applyState() {
        document.documentElement.classList.toggle("ai-design-on-root", enabled);
        document.documentElement.classList.toggle("ai-design-off-root", !enabled);
        document.body.classList.toggle("ai-design-on", enabled);
        document.body.classList.toggle("ai-design-off", !enabled);
        document.body.classList.toggle("live-background-on", enabled);
        document.body.classList.toggle("live-background-off", !enabled);

        toggle.setAttribute("aria-pressed", String(enabled));
        toggle.title = enabled
            ? "Vypnúť AI dizajn a zobraziť pôvodný vzhľad"
            : "Zapnúť AI dizajn";
        status.textContent = enabled ? "ON" : "OFF — pôvodný";
    }

    toggle.addEventListener("click", () => {
        enabled = !enabled;
        window.localStorage.setItem(storageKey, enabled ? "on" : "off");
        applyState();
    });

    applyState();
})();
