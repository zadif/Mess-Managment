function toggleCheckboxes() {
    const boxes = document.querySelectorAll(".item");
    const btn = document.getElementById("toggleBtn");

    // Check if all are already checked
    const allChecked = Array.from(boxes).every(b => b.checked);

    // Toggle state
    boxes.forEach(b => b.checked = !allChecked);

    // Update button text
    btn.textContent = allChecked ? "Mark All" : "Unmark All";
}

function openEditModal(userId) {
    fetch(`/Admin/GetUserConsumption?userId=${userId}`)
        .then(r => r.json())
        .then(data => {
            document.getElementById("modalUserName").textContent = data.userName;
            document.getElementById("modalBody").innerHTML = data.html;
            document.getElementById("editModal").style.display = "block";
            window.currentEditUserId = userId;
        });
}

function closeModal() {
    document.getElementById("editModal").style.display = "none";
}

function saveEdit() {
    const formData = new FormData();
    formData.append("userId", window.currentEditUserId);
    document.querySelectorAll('#modalBody input[type="checkbox"]').forEach(cb => {
        if (cb.checked) {
            formData.append("consumptions", cb.value);
        }
    });

    fetch("/Admin/SaveUserConsumption", {
        method: "POST",
        body: formData,
        headers: { "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value }
    })
        .then(() => {
            location.reload();
        });
}

function deleteTodayConsumption() {
    if (confirm("Delete ALL consumption records for today?")) {
        fetch("/Admin/DeleteTodayConsumption", { method: "POST" })
            .then(() => location.reload());
    }
}

// Close modal if clicked outside
window.onclick = e => { if (e.target === document.getElementById("editModal")) closeModal(); }