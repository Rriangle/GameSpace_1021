document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("sidebar");
    const toggleBtn = document.getElementById("sidebarToggle");

    // 確保 sidebar 始終處於展開狀態（不使用 collapsed class）
    sidebar.classList.remove("collapsed");

    toggleBtn.addEventListener("click", () => {
        sidebar.classList.toggle("collapsed");
        // 保存狀態到 localStorage
        localStorage.setItem("sidebarCollapsed", sidebar.classList.contains("collapsed"));
    });

    // 從 localStorage 讀取狀態
    const isCollapsed = localStorage.getItem("sidebarCollapsed") === "true";
    if (isCollapsed) {
        sidebar.classList.add("collapsed");
    }
});