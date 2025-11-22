document.addEventListener("DOMContentLoaded", () => {

    // DOM Elements
    const toggleCheckbox = document.getElementById("mode-toggle");
    const body = document.body;
    const subtitle = document.getElementById("role-subtitle");
    const textUser = document.getElementById("text-user");
    const textAdmin = document.getElementById("text-admin");

    // Initialize state (default to User)
    textUser.classList.add("text-active");

    // Event Listener for Toggle
    toggleCheckbox.addEventListener("change", () => {
        if (toggleCheckbox.checked) {
            enableAdminMode();
        } else {
            enableUserMode();
        }
    });

    function enableAdminMode() {
        // 1. Add class to body (triggers CSS variable change)
        body.classList.add("admin-mode");

        // 2. Update Text
        subtitle.textContent = "Admin Login";

        // 3. Update Active Label Color
        textUser.classList.remove("text-active");
        textAdmin.classList.add("text-active");

        // 4. (Optional) Update hidden input for MVC form submission if needed
         document.getElementById("hiddenRoleInput").value = "Admin";
    }

    function enableUserMode() {
        // 1. Remove class from body
        body.classList.remove("admin-mode");

        // 2. Update Text
        subtitle.textContent = "User Login";

        // 3. Update Active Label Color
        textAdmin.classList.remove("text-active");
        textUser.classList.add("text-active");

        // 4. (Optional) Update hidden input for MVC form submission if needed
         document.getElementById("hiddenRoleInput").value = "User";
    }
});