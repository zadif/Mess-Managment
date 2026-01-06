document.addEventListener("DOMContentLoaded", () => {
  // Set current date in header
  const dateElement = document.getElementById("currentDate");
  if (dateElement) {
    const options = { weekday: "short", month: "short", day: "numeric" };
    dateElement.textContent = new Date().toLocaleDateString("en-US", options);
  }

  // Mobile sidebar toggle
  const sidebarToggle = document.getElementById("sidebarToggle");
  const sidebar = document.getElementById("sidebar");

  if (sidebarToggle && sidebar) {
    sidebarToggle.addEventListener("click", () => {
      sidebar.classList.toggle("active");

      // Create/toggle overlay for mobile
      let overlay = document.querySelector(".sidebar-overlay");
      if (!overlay) {
        overlay = document.createElement("div");
        overlay.className = "sidebar-overlay";
        document.body.appendChild(overlay);
        overlay.addEventListener("click", () => {
          sidebar.classList.remove("active");
          overlay.classList.remove("active");
        });
      }
      overlay.classList.toggle("active");
    });
  }

  // Fetch and display dashboard stats
  fetchDashboardStats();
});

// Fetch dashboard statistics from API
async function fetchDashboardStats() {
  try {
    // Fetch users count
    const usersResponse = await fetch("/Admin/GetAllUsers");
    if (usersResponse.ok) {
      const users = await usersResponse.json();
      const userCount = document.getElementById("userCount");
      if (userCount) userCount.textContent = users.length || 0;
    }

    // Fetch meals count
    const mealsResponse = await fetch("/Admin/GetAllMeals");
    if (mealsResponse.ok) {
      const meals = await mealsResponse.json();
      const mealCount = document.getElementById("mealCount");
      if (mealCount) mealCount.textContent = meals.length || 0;
    }

    // Set placeholder values for other stats (can be expanded later)
    const billCount = document.getElementById("billCount");
    const todayMeals = document.getElementById("todayMeals");

    if (billCount) billCount.textContent = "--";
    if (todayMeals) todayMeals.textContent = "--";
  } catch (error) {
    console.error("Error fetching dashboard stats:", error);
  }
}
