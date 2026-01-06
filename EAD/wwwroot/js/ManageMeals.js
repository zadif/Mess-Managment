document.addEventListener("DOMContentLoaded", () => {
  loadMeals();

  // Form Submit Handler
  document.getElementById("mealForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    await saveMeal();
  });
});

let allMeals = [];

async function loadMeals() {
  try {
    const response = await fetch("/Admin/GetMealsData");
    const result = await response.json();

    if (result.success) {
      allMeals = result.data;
      renderTable(allMeals);
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error loading meals:", error);
    Swal.fire("Error", "Failed to load meals.", "error");
  }
}

function renderTable(meals) {
  const tbody = document.getElementById("mealsTableBody");
  const emptyState = document.getElementById("emptyState");
  const table = document.getElementById("mealsTable");

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
            <td style="font-weight: 600; color: #fff;">${meal.name}</td>
            <td>
                <span style="background: rgba(255,255,255,0.1); padding: 4px 10px; border-radius: 4px; font-size: 0.8rem;">
                    ${meal.category}
                </span>
            </td>
            <td style="color: var(--accent-orange); font-weight: bold;">Rs. ${meal.price}</td>
            <td>
                <div class="action-buttons">
                    <button onclick="editMeal(${meal.id})" class="btn-action btn-edit">
                        <i class="fas fa-pen"></i> Edit
                    </button>
                    <button onclick="deleteMeal(${meal.id})" class="btn-action btn-delete">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </td>
        `;
    tbody.appendChild(tr);
  });
}

function openModal() {
  document.getElementById("mealForm").reset();
  document.getElementById("mealId").value = "0";
  document.getElementById("modalTitle").textContent = "Add New Meal";
  document.getElementById("mealModal").style.display = "flex";
}

function closeModal() {
  document.getElementById("mealModal").style.display = "none";
}

function editMeal(id) {
  const meal = allMeals.find((m) => m.id === id);
  if (!meal) return;

  document.getElementById("mealId").value = meal.id;
  document.getElementById("mealName").value = meal.name;
  document.getElementById("mealDescription").value = meal.description || "";
  document.getElementById("mealCategory").value = meal.category;
  document.getElementById("mealPrice").value = meal.price;

  document.getElementById("modalTitle").textContent = "Edit Meal";
  document.getElementById("mealModal").style.display = "flex";
}

async function saveMeal() {
  const id = document.getElementById("mealId").value;
  const name = document.getElementById("mealName").value;
  const description = document.getElementById("mealDescription").value;
  const category = document.getElementById("mealCategory").value;
  const price = document.getElementById("mealPrice").value;

  const meal = {
    Id: parseInt(id),
    Name: name,
    Description: description,
    Category: category,
    Price: parseFloat(price),
  };

  try {
    const response = await fetch("/Admin/SaveMealJson", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(meal),
    });

    const result = await response.json();

    if (result.success) {
      Swal.fire("Success", result.message, "success");
      closeModal();
      loadMeals();
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error saving meal:", error);
    Swal.fire("Error", "Failed to save meal.", "error");
  }
}

async function deleteMeal(id) {
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
      const response = await fetch(`/Admin/DeleteMealJson?id=${id}`, {
        method: "POST",
      });
      const data = await response.json();

      if (data.success) {
        Swal.fire("Deleted!", data.message, "success");
        loadMeals();
      } else {
        Swal.fire("Error", data.message, "error");
      }
    } catch (error) {
      console.error("Error deleting meal:", error);
      Swal.fire("Error", "Failed to delete meal.", "error");
    }
  }
}

// Close modal if clicked outside
window.onclick = function (event) {
  const modal = document.getElementById("mealModal");
  if (event.target == modal) {
    closeModal();
  }
};
