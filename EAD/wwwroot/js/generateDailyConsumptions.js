// Function to toggle checkboxes
function toggleCheckboxes() {
    // Selects all checkboxes with the new theme class
    const boxes = document.querySelectorAll(".theme-checkbox");
    const btn = document.getElementById("toggleBtn");

    // Check if all are already checked
    const allChecked = Array.from(boxes).every(b => b.checked);

    // Toggle state
    boxes.forEach(b => b.checked = !allChecked);

    // Update button text while keeping the FontAwesome Icon
    btn.innerHTML = allChecked
        ? '<i class="fa fa-check-square me-2"></i>Mark All'
        : '<i class="fa fa-square-o me-2"></i>Unmark All';
}

function openEditModal(userId) {
    // 1. Fetch JSON Data
    fetch(`/Admin/GetUserConsumption?userId=${userId}`)
        .then(r => r.json())
        .then(data => {

            // Set Header Name
            document.getElementById("modalUserName").textContent = data.userName;
            window.currentEditUserId = data.userId;

            const modalBody = document.getElementById("modalBody");
            modalBody.innerHTML = ""; // Clear previous content

            // 2. Check if data exists
            if (data.items.length === 0) {
                modalBody.innerHTML = `<div class="text-muted text-center p-3">No menu items available for this user.</div>`;
            } else {

                // 3. Loop through data and build HTML
                let htmlContent = "";

                data.items.forEach(item => {
                    const uniqueId = `chk_${data.userId}_${item.menuId}`;
                    const checkedState = item.isChecked ? "checked" : "";

                    // We use the same CSS classes as before: 'consumption-row' and 'theme-checkbox'
                    htmlContent += `
                        <div class="consumption-row" onclick="document.getElementById('${uniqueId}').click()">
                            <input type="checkbox" 
                                   id="${uniqueId}" 
                                   value="${data.userId}-${item.menuId}" 
                                   class="theme-checkbox" 
                                   ${checkedState}
                                   onclick="event.stopPropagation()">
                            
                            <label for="${uniqueId}">
                   <span class="d-block text-white fw-bold me-2">${item.mealType}</span>
                                <span class="text-muted small">${item.itemName}</span>
                            </label>
                        </div>
                    `;
                });

                modalBody.innerHTML = htmlContent;
            }

            // 4. Show Modal (Flex for centering)
            document.getElementById("editModal").style.display = "flex";
        })
        .catch(err => console.error("Error fetching data:", err));
}
// Close Modal
function closeModal() {
    document.getElementById("editModal").style.display = "none";
}

// Save changes from Modal
function saveEdit() {
    const formData = new FormData();
    formData.append("userId", window.currentEditUserId);

    // Selects checkboxes inside the modal body
    document.querySelectorAll('#modalBody input[type="checkbox"]').forEach(cb => {
        if (cb.checked) {
            formData.append("consumptions", cb.value);
        }
    });

    fetch("/Admin/SaveUserConsumption", {
        method: "POST",
        body: formData,
        headers: {
            "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(() => {
            location.reload();
        });
}

// Delete all consumption for today
function deleteTodayConsumption() {
    if (confirm("Delete ALL consumption records for today?")) {
        fetch("/Admin/DeleteTodayConsumption", { method: "POST" })
            .then(() => location.reload());
    }
}

// Close modal if clicked outside (on the backdrop)
window.onclick = e => {
    if (e.target === document.getElementById("editModal")) {
        closeModal();
    }
}