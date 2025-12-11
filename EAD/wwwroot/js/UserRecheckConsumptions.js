let currentId = 0;

function openRecheckModal(id) {
    currentId = id;
    document.getElementById("recheckMessage").value = "";
    document.getElementById("recheckModal").style.display = "flex";
}

function closeRecheckModal() {
    document.getElementById("recheckModal").style.display = "none";
}

function sendRecheckRequest() {
    const msg = document.getElementById("recheckMessage").value.trim();
    if (!msg) {
        alert("Please write a reason");
        return;
    }



    // 2. Prepare Form Data
    const formData = new URLSearchParams();
    formData.append("id", currentId);
    // Even if controller doesn't use message yet, we can send it just in case
    // formData.append("message", msg); 

    fetch("/UserMenu/RecheckConsumptions", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
        },
        body: formData.toString()
    })
        .then(r => r.json())
        .then(result => {
            // 3. Check result (Controller returns 1 for success)
            if (result === 1) {

                // --- UPDATE UI WITHOUT RELOAD ---

                // Update Status Column
                const statusCell = document.getElementById("status-" + currentId);
                if (statusCell) {
                    statusCell.innerHTML = '<span class="badge bg-danger">Recheck Request Sent</span>';
                }

                // Update Action Column
                const actionCell = document.getElementById("action-" + currentId);
                if (actionCell) {
                    actionCell.innerHTML = '<span class="text-muted">Request Sent</span>';
                }

                // Close modal and notify user
                closeRecheckModal();
                // Optional: Show a small toast instead of alert to be smoother
                // alert("Request sent successfully."); 

            } else {
                alert("Failed to send request. Please try again.");
            }
        })
        .catch(err => {
            console.error("Fetch error:", err);
            alert("Network error occurred.");
        });
}

// Close modal on outside click
window.onclick = function (e) {
    if (e.target.id === "recheckModal") closeRecheckModal();
}