let isSaving = false; // Prevent double submissions
let currentData = null; // Store current data for reference

// Get date from input
function getDate() {
    const dateInput = document.getElementById("consumptionDate");
    return dateInput ? dateInput.value : new Date().toISOString().split("T")[0];
}

// Load Data via AJAX
async function loadData() {
    const date = getDate();
    const spinner = document.getElementById("loadingSpinner");
    const content = document.getElementById("contentContainer");
    const noData = document.getElementById("noDataMessage");
    const alertContainer = document.getElementById("alertContainer");

    // Reset UI
    alertContainer.innerHTML = "";
    spinner.style.display = "block";
    content.style.display = "none";
    noData.style.display = "none";

    try {
        const response = await fetch(
            `/Admin/GetDailyConsumptionsData?date=${date}`
        );
        if (!response.ok) throw new Error("Failed to fetch data");

        const result = await response.json();

        if (result.success) {
            currentData = result;
            document.getElementById("displayDate").textContent = result.displayDate;

            if (!result.menus || result.menus.length === 0) {
                noData.style.display = "block";
                noData.innerHTML =
                    '<i class="fa fa-exclamation-triangle me-2"></i> No menu items found for this date.';
            } else if (!result.users || result.users.length === 0) {
                noData.style.display = "block";
                noData.innerHTML =
                    '<i class="fa fa-users-slash me-2"></i> No active users found.';
            } else {
                renderTable(result);
                content.style.display = "block";
                updateSelectedCount();
            }
        } else {
            showAlert(result.message || "Failed to load data.", "danger");
        }
    } catch (error) {
        console.error(error);
        showAlert("Error loading data. Please try again.", "danger");
    } finally {
        spinner.style.display = "none";
    }
}

// Render Table
function renderTable(data) {
    const headerRow = document.getElementById("tableHeaderRow");
    const tableBody = document.getElementById("tableBody");

    // 1. Build Header
    let headerHtml = `<th class="ps-4">User</th><th>Type</th>`;
    data.menus.forEach((menu) => {
        headerHtml += `
            <th class="text-center">
                <div class="d-flex flex-column">
                    <span class="menu-type">${menu.mealType}</span>
                    <small class="text-muted">${menu.mealItem.name} (Rs. ${menu.mealItem.price})</small>
                </div>
            </th>`;
    });
    headerHtml += `<th class="text-center pe-4">Actions</th>`;
    headerRow.innerHTML = headerHtml;

    // 2. Build Body
    let bodyHtml = "";
    const alreadyGeneratedSet = new Set(data.alreadyGenerated);

    data.users.forEach((user) => {
        const isGenerated = alreadyGeneratedSet.has(user.id);
        const userTypeLabel = user.userType === 2 ? "Food + Drinks" : "Drinks Only";
        const badgeClass = user.userType === 2 ? "badge-food" : "badge-drink";

        bodyHtml += `<tr class="${isGenerated ? "row-generated" : ""
            }" data-user-id="${user.id}">
            <td class="ps-4">
                <div class="fw-bold text-white">${user.name}</div>
                <small class="text-muted">#${user.id}</small>
            </td>
            <td>
                <span class="badge ${badgeClass}">${userTypeLabel}</span>
            </td>`;

        if (isGenerated) {
            bodyHtml += `
                <td colspan="${data.menus.length}" class="text-center">
                    <span class="badge badge-generated">
                        <i class="fa fa-check me-1"></i> Generated
                    </span>
                </td>`;
        } else {
            data.menus.forEach((menu) => {
                // Logic: UserType 2 can eat everything. UserType 1 cannot eat "Food".
                // Note: JS comparison should be case-insensitive
                const isFood = menu.mealItem.category.toLowerCase() === "food";
                const canConsume = user.userType === 2 || !isFood;

                if (canConsume) {
                    bodyHtml += `
                        <td class="text-center">
                            <div class="custom-check-wrapper">
                                <input type="checkbox"
                                       class="item theme-checkbox"
                                       data-value="${user.id}-${menu.mealItemId}"
                                       checked 
                                       onchange="updateSelectedCount()" />
                            </div>
                        </td>`;
                } else {
                    bodyHtml += `
                        <td class="text-center">
                            <span class="text-muted opacity-25"><i class="fa fa-ban"></i></span>
                        </td>`;
                }
            });
        }

        bodyHtml += `<td class="text-center pe-4">`;
        if (isGenerated) {
            bodyHtml += `
                <div class="btn-group">
                    <button type="button" class="btn btn-sm btn-icon-blue" onclick="openEditModal(${user.id
                })" title="Edit">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-icon-red ms-2" onclick="deleteUserConsumption(${user.id
                }, '${user.name.replace(/'/g, "\\'")}')">
                        <i class="fa fa-trash"></i>
                    </button>
                </div>`;
        }
        bodyHtml += `</td></tr>`;
    });

    tableBody.innerHTML = bodyHtml;
}

// Update selected count
function updateSelectedCount() {
    const countElement = document.getElementById("selectedCount");
    if (!countElement) return;

    const checkedBoxes = document.querySelectorAll(".theme-checkbox:checked");
    countElement.textContent = checkedBoxes.length;
}

// Toggle checkboxes
function toggleCheckboxes() {
    const boxes = document.querySelectorAll(
        "#tableBody tr:not(.row-generated) .theme-checkbox"
    );
    if (boxes.length === 0) return;

    const allChecked = Array.from(boxes).every((b) => b.checked);
    boxes.forEach((b) => (b.checked = !allChecked));

    const btn = document.getElementById("toggleBtn");
    btn.innerHTML = allChecked
        ? '<i class="fa fa-check-square me-2"></i>Mark All'
        : '<i class="fa fa-square-o me-2"></i>Unmark All';

    updateSelectedCount();
}

// Save All
async function saveAllConsumptions() {
    if (isSaving) return;

    const checkedBoxes = document.querySelectorAll(".theme-checkbox:checked");
    if (checkedBoxes.length === 0) {
        showAlert("No items selected to save.", "warning");
        return;
    }

    const consumptions = Array.from(checkedBoxes).map((cb) => cb.dataset.value);
    const formData = new FormData();
    consumptions.forEach((val) => formData.append("consumptions", val));
    formData.append("date", getDate());

    try {
        isSaving = true;
        disableButtons(true);

        const response = await fetch("/Admin/GenerateDailyConsumptions", {
            method: "POST",
            body: formData,
        });

        const result = await response.json();

        if (result.success) {
            showAlert(`Success: ${result.message}`, "success");
            loadData(); // Refresh data without reload
        } else {
            showAlert(result.message || "Save failed.", "danger");
        }
    } catch (err) {
        console.error(err);
        showAlert("Network error. Please try again.", "danger");
    } finally {
        isSaving = false;
        disableButtons(false);
    }
}

// Delete All Today
async function deleteTodayConsumption() {
    if (
        !confirm("Delete ALL consumption records for today? This cannot be undone.")
    )
        return;

    const formData = new FormData();
    formData.append("date", getDate());

    try {
        disableButtons(true);
        const response = await fetch("/Admin/DeleteTodayConsumption", {
            method: "POST",
            body: formData,
        });

        const result = await response.json();

        if (result.success) {
            showAlert(result.message, "success");
            loadData(); // Refresh data without reload
        } else {
            showAlert(result.message, "danger");
        }
    } catch (err) {
        showAlert("Delete failed.", "danger");
    } finally {
        disableButtons(false);
    }
}

// Delete Single User
async function deleteUserConsumption(userId, userName) {
    if (!confirm(`Delete consumption for ${userName} today?`)) return;

    const formData = new FormData();
    formData.append("userId", userId);
    formData.append("date", getDate());

    try {
        const response = await fetch("/Admin/DeleteUserConsumption", {
            method: "POST",
            body: formData,
        });

        const result = await response.json();

        if (result.success) {
            showAlert(result.message, "success");
            loadData(); // Refresh data without reload
        } else {
            showAlert(result.message, "danger");
        }
    } catch (err) {
        showAlert("Delete failed.", "danger");
    }
}

// Open Edit Modal
async function openEditModal(userId) {
    try {
        const date = getDate();
        const response = await fetch(
            `/Admin/GetUserConsumption?userId=${userId}&date=${date}`
        );

        if (!response.ok) throw new Error("Fetch failed");

        const data = await response.json();

        document.getElementById("modalUserName").textContent = data.userName;
        window.currentEditUserId = data.userId;

        const modalBody = document.getElementById("modalBody");
        modalBody.innerHTML = "";

        if (!data.items || data.items.length === 0) {
            modalBody.innerHTML = `<div class="text-muted text-center p-3">No menu items available.</div>`;
        } else {
            let html = "";
            data.items.forEach((item) => {
                const id = `chk_${userId}_${item.menuId}`;
                html += `
                    <div class="consumption-row" onclick="document.getElementById('${id}').click()">
                        <input type="checkbox" id="${id}" value="${userId}-${item.menuId
                    }"
                               class="theme-checkbox" ${item.isChecked ? "checked" : ""
                    }
                               onclick="event.stopPropagation()">
                        <label for="${id}">
                             <span class="d-block text-white fw-bold">${item.mealType
                    }:  &nbsp;&nbsp;</span>
                            <span class="text-muted small">${item.itemName
                    }</span>
                        </label>
                    </div>`;
            });
            modalBody.innerHTML = html;
        }

        document.getElementById("editModal").style.display = "flex";
    } catch (err) {
        showAlert("Failed to load user data.", "danger");
    }
}

// Save Edit
async function saveEdit() {
    if (isSaving) return;

    const checked = document.querySelectorAll(
        '#modalBody input[type="checkbox"]:checked'
    );
    const consumptions = Array.from(checked).map((cb) => cb.value);

    const formData = new FormData();
    formData.append("userId", window.currentEditUserId);
    consumptions.forEach((v) => formData.append("consumptions", v));
    formData.append("date", getDate());

    try {
        isSaving = true;
        const btn = document.getElementById("modalSaveBtn");
        btn.disabled = true;

        const response = await fetch("/Admin/SaveUserConsumption", {
            method: "POST",
            body: formData,
        });

        const result = await response.json();

        if (result.success) {
            showAlert("User consumption updated successfully.", "success");
            closeModal();
            loadData(); // Refresh data without reload
        } else {
            showAlert(result.message || "Update failed.", "danger");
        }
    } catch (err) {
        showAlert("Network error.", "danger");
    } finally {
        isSaving = false;
        document.getElementById("modalSaveBtn").disabled = false;
    }
}

// Helper: Show Alert
function showAlert(message, type = "info") {
    const container = document.getElementById("alertContainer");
    container.innerHTML = `
        <div class="alert alert-theme-${type === "success"
            ? "success"
            : type === "danger"
                ? "danger"
                : "warning"
        } alert-dismissible fade show">
            <i class="fa fa-${type === "success" ? "check-circle" : "exclamation-circle"
        } me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`;
}

// Helper: Disable Buttons
function disableButtons(disabled) {
    const saveBtn = document.getElementById("saveBtn");
    const deleteBtn = document.getElementById("deleteAllBtn");
    if (saveBtn) saveBtn.disabled = disabled;
    if (deleteBtn) deleteBtn.disabled = disabled;
}

// Helper: Close Modal
function closeModal() {
    document.getElementById("editModal").style.display = "none";
}

// Close on backdrop click
window.onclick = (e) => {
    if (e.target.id === "editModal") closeModal();
};
