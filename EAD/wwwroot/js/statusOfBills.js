function verifyBill(billId, button) {
    if (!confirm("Verify this bill as paid?")) return;

    button.disabled = true;
    button.innerHTML = "Verifying...";

    const formData = new FormData();
    formData.append("billId", billId);  // This matches your controller parameter

    fetch("/Admin/VerifyBill", {
        method: "POST",
        body: formData,
       
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                const row = button.closest("tr");
                row.cells[6].innerHTML = '<span class="badge bg-success">Verified</span>';
                row.cells[7].innerHTML = '<span class="text-success fw-bold">Verified</span>';
            } else {
                button.disabled = false;
                button.innerHTML = "Verify Payment";
                alert("Error verifying bill.");
            }
        })
        .catch(() => {
            button.disabled = false;
            button.innerHTML = "Verify Payment";
            alert("Network error!");
        });
}