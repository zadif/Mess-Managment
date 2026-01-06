document.addEventListener("DOMContentLoaded", () => {
  loadDailyMeals();
  loadMealItems(); // Populate dropdown

  // Form Submit Handler
  document
    .getElementById("dailyMealForm")
    .addEventListener("submit", async (e) => {
      e.preventDefault();
      await saveDailyMeal();
    });
});

let allDailyMeals = [];
let availableMeals = [];

async function loadDailyMeals() {
  try {
    const response = await fetch("/Admin/GetDailyMenusData");
    const result = await response.json();

    if (result.success) {
      allDailyMeals = result.data;
      renderTable(allDailyMeals);
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error loading daily meals:", error);
    Swal.fire("Error", "Failed to load daily meals.", "error");
  }
}

async function loadMealItems() {
  try {
    const response = await fetch("/Admin/GetMealsData");
    const result = await response.json();

    if (result.success) {
      availableMeals = result.data;
      const select = document.getElementById("mealItemId");
      select.innerHTML = "";
      availableMeals.forEach((meal) => {
        const option = document.createElement("option");
        option.value = meal.id;
        option.textContent = `${meal.name} (${meal.category}) - Rs. ${meal.price}`;
        select.appendChild(option);
      });
    }
  } catch (error) {
    console.error("Error loading meal items:", error);
  }
}

function renderTable(meals) {
  const tbody = document.getElementById("dailyMealsTableBody");
  const emptyState = document.getElementById("emptyState");
  const table = document.getElementById("dailyMealsTable");

  tbody.innerHTML = "";

  if (meals.length === 0) {
    table.style.display = "none";
    emptyState.style.display = "block";
    return;
  }

  table.style.display = "table";
  emptyState.style.display = "none";

  meals.forEach((meal) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
            <td>#${meal.id}</td>
            <td>${meal.dayOfWeek}</td>
            <td>${meal.mealType}</td>
            <td>${meal.mealItemName}</td>
            <td>
                <div class="action-buttons">
                    <button onclick="editDailyMeal(${meal.id})" class="btn-action btn-edit">
                        <i class="fas fa-pen"></i> Edit
                    </button>
                    <button onclick="deleteDailyMeal(${meal.id})" class="btn-action btn-delete">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </td>
        `;
    tbody.appendChild(tr);
  });
}

function openModal() {
  document.getElementById("dailyMealForm").reset();
  document.getElementById("dailyMealId").value = "0";
  document.getElementById("modalTitle").textContent = "Set Daily Meal";
  document.getElementById("dailyMealModal").style.display = "flex";
}

function closeModal() {
  document.getElementById("dailyMealModal").style.display = "none";
}

function editDailyMeal(id) {
  const meal = allDailyMeals.find((m) => m.id === id);
  if (!meal) return;

  document.getElementById("dailyMealId").value = meal.id;
  document.getElementById("dayOfWeek").value = meal.dayOfWeek;
  document.getElementById("mealType").value = meal.mealType;
  document.getElementById("mealItemId").value = meal.mealItemId;

  document.getElementById("modalTitle").textContent = "Edit Daily Meal";
  document.getElementById("dailyMealModal").style.display = "flex";
}

async function saveDailyMeal() {
  const id = document.getElementById("dailyMealId").value;
  const dayOfWeek = document.getElementById("dayOfWeek").value;
  const mealType = document.getElementById("mealType").value;
  const mealItemId = document.getElementById("mealItemId").value;

  const dailyMenu = {
    Id: parseInt(id),
    DayOfWeek: dayOfWeek,
    MealType: mealType,
    MealItemId: parseInt(mealItemId),
  };

  try {
    const response = await fetch("/Admin/SaveDailyMenuJson", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(dailyMenu),
    });

    const result = await response.json();

    if (result.success) {
      Swal.fire("Success", result.message, "success");
      closeModal();
      loadDailyMeals();
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error saving daily meal:", error);
    Swal.fire("Error", "Failed to save daily meal.", "error");
  }
}

async function deleteDailyMeal(id) {
  const result = await Swal.fire({
    title: "Are you sure?",
    text: "You won't be able to revert this!",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#3085d6",
    confirmButtonText: "Yes, delete it!",
  });

  if (result.isConfirmed) {
    try {
      const response = await fetch(`/Admin/DeleteDailyMenuJson?id=${id}`, {
        method: "POST",
      });
      const data = await response.json();

      if (data.success) {
        Swal.fire("Deleted!", data.message, "success");
        loadDailyMeals();
      } else {
        Swal.fire("Error", data.message, "error");
      }
    } catch (error) {
      console.error("Error deleting daily meal:", error);
      Swal.fire("Error", "Failed to delete daily meal.", "error");
    }
  }
}

// Close modal if clicked outside
window.onclick = function (event) {
  const modal = document.getElementById("dailyMealModal");
  if (event.target == modal) {
    closeModal();
  }
};
