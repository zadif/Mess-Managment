document.addEventListener("DOMContentLoaded", () => {
  // DOM Elements - New Tab-based UI
  const tabUser = document.getElementById("tab-user");
  const tabAdmin = document.getElementById("tab-admin");
  const tabIndicator = document.querySelector(".tab-indicator");
  const hiddenRoleInput = document.getElementById("hiddenRoleInput");
  const subtitle = document.getElementById("role-subtitle");

  // Initialize state (default to User)
  setActiveRole("User");

  // Event Listeners for Role Tabs
  tabUser.addEventListener("click", () => setActiveRole("User"));
  tabAdmin.addEventListener("click", () => setActiveRole("Admin"));

  function setActiveRole(role) {
    // Update active tab styling
    tabUser.classList.toggle("active", role === "User");
    tabAdmin.classList.toggle("active", role === "Admin");

    // Move indicator
    if (tabIndicator) {
      tabIndicator.style.transform =
        role === "Admin" ? "translateX(100%)" : "translateX(0)";
    }

    // Update subtitle text
    if (subtitle) {
      subtitle.textContent = `Sign in as ${role}`;
    }

    // Update hidden input for form submission
    if (hiddenRoleInput) {
      hiddenRoleInput.value = role;
    }
  }
});

// Password visibility toggle
function togglePasswordVisibility() {
  const passwordInput = document.getElementById("password");
  const eyeIcon = document.getElementById("eye-icon");

  if (passwordInput.type === "password") {
    passwordInput.type = "text";
    eyeIcon.classList.remove("fa-eye");
    eyeIcon.classList.add("fa-eye-slash");
  } else {
    passwordInput.type = "password";
    eyeIcon.classList.remove("fa-eye-slash");
    eyeIcon.classList.add("fa-eye");
  }
}

function validateForm() {
  const email = document.getElementById("username").value;
  const password = document.getElementById("password").value;
  const role = document.getElementById("hiddenRoleInput").value;

  // Admin bypass: Admin login is hardcoded as "a"/"a" in backend, so we skip validation for Admin
  if (role === "Admin") {
    return true;
  }

  if (!email.includes("@")) {
    alert('Please enter a valid email address containing "@".');
    return false;
  }

  if (password.length < 3) {
    alert("Password must be at least 3 characters long.");
    return false;
  }

  return true;
}
