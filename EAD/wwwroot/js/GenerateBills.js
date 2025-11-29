    function generateBillForUser(button) {
    const userId = button.getAttribute("data-userid");
    const total = button.getAttribute("data-total");

    button.disabled = true;
    button.innerHTML = "Generating...";

    const formData = new FormData();
    formData.append("userId", userId);
    formData.append("total", total);

    fetch("/Admin/GenerateBills", {
        method: "POST",
    body: formData,
   
    })
    .then(r => r.json())
    .then(data => {
        if (data.success) {
            // Remove all rows for this user
            const row = button.closest("tr");
    let prev = row.previousElementSibling;
    while (prev && !prev.classList.contains("table-primary")) {
                const temp = prev;
    prev = prev.previousElementSibling;
    temp.remove();
            }
    row.remove();
    alert("Bill generated successfully!");
        } else {
        button.disabled = false;
    button.innerHTML = "Generate Bill";
    alert("Server error!");
        }
    })
    .catch(() => {
        button.disabled = false;
    button.innerHTML = "Generate Bill";
    alert("Network error!");
    });
}

    // Generate All Bills
    document.getElementById("generateAllBillsBtn")?.addEventListener("click", async function () {
    if (!confirm("Generate bills for ALL users?")) return;

    const mainBtn = this;
    mainBtn.disabled = true;
    mainBtn.innerHTML = "Generating All...";
    const progress = document.getElementById("progressText");

    const buttons = document.querySelectorAll("tr.table-primary button[data-userid]");
    let success = 0, failed = 0;

    for (let i = 0; i < buttons.length; i++) {
        const btn = buttons[i];
    const userId = btn.getAttribute("data-userid");
    const total = btn.getAttribute("data-total");

    progress.innerHTML = `Processing ${i + 1} of ${buttons.length}...`;

    const formData = new FormData();
    formData.append("userId", userId);
    formData.append("total", total);

    try {
            const res = await fetch("/Admin/GenerateBills", {
        method: "POST",
    body: formData,
   
            });
    const data = await res.json();
    if (data.success) {
                // Remove user block
                const row = btn.closest("tr");
    let prev = row.previousElementSibling;
    while (prev && !prev.classList.contains("table-primary")) {
        prev.remove();
    prev = row.previousElementSibling;
                }
    row.remove();
    success++;
            } else failed++;
        } catch {failed++; }
    }

    progress.innerHTML = `<strong>Done! ${success} succeeded, ${failed} failed.</strong>`;
    mainBtn.innerHTML = "All Bills Generated!";
    mainBtn.classList.replace("btn-danger", "btn-success");
});
