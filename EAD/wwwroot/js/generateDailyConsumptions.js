let isSaving = false; // Prevent double submissions

// Update selected count in real-time
function updateSelectedCount() {
    const countElement = document.getElementById('selectedCount');

    // Safety check: if the element doesn't exist (e.g., no users/menus), do nothing
    if (!countElement) {
        return;
    }

    const checkedBoxes = document.querySelectorAll('.theme-checkbox:checked');
    const count = checkedBoxes.length;
    countElement.textContent = count;
}

// Toggle all checkboxes (only non-generated rows)
function toggleCheckboxes() {
    const boxes = document.querySelectorAll('tr:not(.row-generated) .theme-checkbox');
    const allChecked = Array.from(boxes).every(b => b.checked);

    boxes.forEach(b => b.checked = !allChecked);

    const btn = document.getElementById('toggleBtn');
    btn.innerHTML = allChecked
        ? '<i class="fa fa-check-square me-2"></i>Mark All'
        : '<i class="fa fa-square-o me-2"></i>Unmark All';

    updateSelectedCount();
}


// Save all consumptions (main Save button)
async function saveAllConsumptions() {
    if (isSaving) return;

    const checkedBoxes = document.querySelectorAll('.theme-checkbox:checked');
    if (checkedBoxes.length === 0) {
        showAlert('No items selected to save.', 'warning');
        return;
    }

    const consumptions = Array.from(checkedBoxes).map(cb => cb.dataset.value);

    const formData = new FormData();
    consumptions.forEach(val => formData.append('consumptions', val));


    let selectedDate = getDate();

 

    formData.append('date', selectedDate);

    try {
        isSaving = true;
        disableButtons(true);

        const response = await fetch('/Admin/GenerateDailyConsumptions', {
            method: 'POST',
            body: formData,
            
        });

        const result = await response.json();

        if (result.success) {
            showAlert(`Success: ${result.message}`, 'success');
            setTimeout(() => location.reload(), 1500);
        } else {
            showAlert(result.message || 'Save failed.', 'danger');
        }
    } catch (err) {
        console.error(err);
        showAlert('Network error. Please try again.', 'danger');
    } finally {
        isSaving = false;
        disableButtons(false);
    }
}

// Open Edit Modal
async function openEditModal(userId) {
    try {
        const response = await fetch(`/Admin/GetUserConsumption?userId=${userId}`);
        if (!response.ok) throw new Error('Failed to fetch');

        const data = await response.json();

        document.getElementById('modalUserName').textContent = data.userName;
        window.currentEditUserId = data.userId;

        const modalBody = document.getElementById('modalBody');
        modalBody.innerHTML = '';

        if (data.items.length === 0) {
            modalBody.innerHTML = `<div class="text-muted text-center p-3">No menu items available.</div>`;
        } else {
            let html = '';
            data.items.forEach(item => {
                const id = `chk_${userId}_${item.menuId}`;
                html += `
                    <div class="consumption-row" onclick="document.getElementById('${id}').click()">
                        <input type="checkbox" id="${id}" value="${userId}-${item.menuId}"
                               class="theme-checkbox" ${item.isChecked ? 'checked' : ''}
                               onclick="event.stopPropagation()">
                        <label for="${id}">
                             <span class="d-block text-white fw-bold">${item.mealType}:  &nbsp;&nbsp;</span>
                            <span class="text-muted small">${item.itemName}</span>
                        </label>
                    </div>`;
            });
            modalBody.innerHTML = html;
        }

        document.getElementById('editModal').style.display = 'flex';
    } catch (err) {
        showAlert('Failed to load user data.', 'danger');
    }
}

// Save individual user edit
async function saveEdit() {
    if (isSaving) return;

    const checked = document.querySelectorAll('#modalBody input[type="checkbox"]:checked');
    const consumptions = Array.from(checked).map(cb => cb.value);

    const formData = new FormData();
    formData.append('userId', window.currentEditUserId);
    consumptions.forEach(v => formData.append('consumptions', v));


    try {
        isSaving = true;
        document.getElementById('modalSaveBtn').disabled = true;

        const response = await fetch('/Admin/SaveUserConsumption', {
            method: 'POST',
            body: formData,
        });

        const result = await response.json();

        if (result.success) {
            showAlert('User consumption updated successfully.', 'success');
            closeModal();
            setTimeout(() => location.reload(), 1000);
        } else {
            showAlert(result.message || 'Update failed.', 'danger');
        }
    } catch (err) {
        showAlert('Network error.', 'danger');
    } finally {
        isSaving = false;
        document.getElementById('modalSaveBtn').disabled = false;
    }
}

// Delete single user
async function deleteUserConsumption(userId, userName) {
    if (!confirm(`Delete consumption for ${userName} today?`)) return;

    const formData = new FormData();
    formData.append('userId', userId);


    try {
        const response = await fetch('/Admin/DeleteUserConsumption', {
            method: 'POST',
            body: formData,
        });

        const result = await response.json();
        showAlert(result.message, result.success ? 'success' : 'danger');

        if (result.success) {
            setTimeout(() => location.reload(), 1200);
        }
    } catch (err) {
        showAlert('Delete failed.', 'danger');
    }
}

// Delete all today
async function deleteTodayConsumption() {
    if (!confirm('Delete ALL consumption records for today? This cannot be undone.')) return;

    const formData = new FormData();
    let selectedDate = getDate();
    formData.append('date', selectedDate);
    try {
        disableButtons(true);
        const response = await fetch('/Admin/DeleteTodayConsumption', {
            method: 'POST',
            body: formData,

        });

        const result = await response.json();
        showAlert(result.message, result.success ? 'success' : 'danger');

        if (result.success) {
            setTimeout(() => location.reload(), 1500);
        }
    } catch (err) {
        showAlert('Delete failed.', 'danger');
    } finally {
        disableButtons(false);
    }
}

// Utility: Show alert
function showAlert(message, type = 'info') {
    const container = document.getElementById('alertContainer');
    container.innerHTML = `
        <div class="alert alert-theme-${type === 'success' ? 'success' : type === 'danger' ? 'danger' : 'warning'} alert-dismissible fade show">
            <i class="fa fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>`;
}

// Disable buttons during operation
function disableButtons(disabled) {
    document.getElementById('saveBtn').disabled = disabled;
    document.getElementById('deleteAllBtn').disabled = disabled;
}

// Close modal
function closeModal() {
    document.getElementById('editModal').style.display = 'none';
}

// Close on backdrop click
window.onclick = (e) => {
    if (e.target.id === 'editModal') closeModal();
};

// Initialize count on load
document.addEventListener('DOMContentLoaded', () => {
    updateSelectedCount();

    // Update count when any checkbox changes
    document.querySelectorAll('.theme-checkbox').forEach(cb => {
        cb.addEventListener('change', updateSelectedCount);
    });
});


  
    // Optional: Trigger on Enter key in date field
    document.getElementById('consumptionDate').addEventListener('keydown', function(e) {
        if (e.key === 'Enter') {
        e.preventDefault();
    loadConsumptionForDate();
        }
    });



async function loadConsumptionForDate() {
    const dateInput = document.getElementById('consumptionDate');
    const selectedDate = dateInput.value;
    if (!selectedDate) {
        showAlert('Please select a valid date.', 'warning');
        return;
    }

    const url = `/Admin/generateDailyConsumptions?date=${selectedDate}`;

    try {
        const response = await fetch(url, { method: 'GET' });
        if (!response.ok) throw new Error('Failed to load data');

        const html = await response.text();

        document.querySelector('.mess-container').innerHTML =
            (/<!DOCTYPE html>/.test(html)
                ? html.match(/<body[^>]*>([\s\S]*)<\/body>/i)[1]
                : html);

        // Update displayed date
        const dateObj = new Date(selectedDate);
        const formatted = dateObj.toLocaleDateString('en-US', {
            weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
        });
        document.getElementById('displayDate').textContent = formatted;

        showAlert(`Consumption data loaded for ${formatted}`, 'success');
        updateSelectedCount();

        // === FIX: Re-attach the click listener to the NEW button ===
        const newLoadBtn = document.getElementById('loadDateBtn');
        if (newLoadBtn) {
            newLoadBtn.addEventListener('click', loadConsumptionForDate);
        }
        // === Also sync the date input so saving uses correct date ===
        const newDateInput = document.getElementById('consumptionDate');
        if (newDateInput) {
            newDateInput.value = selectedDate;
        }

    } catch (error) {
        console.error(error);
        showAlert('Error loading consumption data. Please try again.', 'danger');
    }
}
// Re-attach event listener to the newly loaded button
const loadBtn = document.getElementById('loadDateBtn');
if (loadBtn) {
    loadBtn.addEventListener('click', loadConsumptionForDate);
}



function getDate() {
    const displayDateEl = document.getElementById('displayDate');
    let selectedDate = '';

    // Priority: data-date attribute (exact, no parsing needed)
    if (displayDateEl?.dataset.date) {
        selectedDate = displayDateEl.dataset.date;
    }
    // Fallback: parse visible text safely in local time
    else if (displayDateEl) {
        const text = displayDateEl.textContent.trim();
        const dateObj = new Date(text);
        if (!isNaN(dateObj.getTime())) {
            const year = dateObj.getFullYear();
            const month = String(dateObj.getMonth() + 1).padStart(2, '0');
            const day = String(dateObj.getDate()).padStart(2, '0');
            selectedDate = `${year}-${month}-${day}`;
        }
    }

    // Final fallback: today's date in LOCAL time (no UTC shift)
    if (!selectedDate) {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth()   +1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');
        selectedDate = `${year}-${month}-${day}`;
    }

    return selectedDate;
}
