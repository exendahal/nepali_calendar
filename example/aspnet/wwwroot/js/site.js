// ---- scroll-spy: highlight the drawer item for the section in view ----
(() => {
    const navLinks = Array.from(document.querySelectorAll(".drawer nav a[href^='#']"));
    if (navLinks.length === 0) return;

    const linkBySlug = new Map(navLinks.map((a) => [a.getAttribute("href").slice(1), a]));
    const sections = Array.from(document.querySelectorAll("main section[id]"))
        .filter((s) => linkBySlug.has(s.id));
    if (sections.length === 0) return;

    let current = null;

    const setActive = (slug) => {
        if (!slug || slug === current || !linkBySlug.has(slug)) return;
        current = slug;
        navLinks.forEach((a) => {
            a.classList.remove("active");
            a.removeAttribute("aria-current");
        });
        const link = linkBySlug.get(slug);
        link.classList.add("active");
        link.setAttribute("aria-current", "true");
        link.scrollIntoView({ block: "nearest" });
    };

    // A thin band near the top of the viewport: the section crossing it counts as "current".
    const observer = new IntersectionObserver(
        (entries) => {
            const visible = entries
                .filter((e) => e.isIntersecting)
                .sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
            if (visible.length > 0) setActive(visible[0].target.id);
        },
        { rootMargin: "-15% 0px -70% 0px", threshold: 0 },
    );

    sections.forEach((s) => observer.observe(s));
    setActive(sections[0].id);

    // The last section's top can pass the observed band before the page finishes scrolling
    // (nothing left below to push it through) — force it active once we hit the bottom.
    const checkBottom = () => {
        const atBottom = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 2;
        if (atBottom) setActive(sections[sections.length - 1].id);
    };
    window.addEventListener("scroll", checkBottom, { passive: true });

    navLinks.forEach((a) => {
        a.addEventListener("click", () => setActive(a.getAttribute("href").slice(1)));
    });
})();

// ---- top app bar: raise on scroll (Material elevation-on-scroll) ----
(() => {
    const bar = document.getElementById("app-bar");
    if (!bar) return;

    const update = () => bar.classList.toggle("raised", window.scrollY > 0);
    update();
    window.addEventListener("scroll", update, { passive: true });
})();

// ---- Material ripple ----
(() => {
    const targets = document.querySelectorAll("[data-ripple]");
    targets.forEach((el) => {
        el.addEventListener("click", (event) => {
            const rect = el.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const ripple = document.createElement("span");
            ripple.className = "ripple";
            ripple.style.width = ripple.style.height = `${size}px`;
            ripple.style.left = `${event.clientX - rect.left - size / 2}px`;
            ripple.style.top = `${event.clientY - rect.top - size / 2}px`;
            el.appendChild(ripple);
            ripple.addEventListener("animationend", () => ripple.remove());
        });
    });
})();
