let recheckModalInstance = null;
let currentId = 0;

// 1. Function to Open Modal
function openRecheckModal(id) {
    currentId = id;

    const modalElement = document.getElementById("recheckModalBs");

    if (modalElement) {
        // Initialize Bootstrap Modal via JS API
        if (typeof bootstrap !== 'undefined') {
            recheckModalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
            recheckModalInstance.show();
        } else {
            // Fallback for Vanilla JS if Bootstrap JS isn't loaded
            modalElement.style.display = "block";
            modalElement.classList.add("show");
            document.body.classList.add("modal-open");
        }
    }
}

// 2. Function to Close Modal
function closeRecheckModal() {
    if (recheckModalInstance) {
        recheckModalInstance.hide();
    } else {
        const modalElement = document.getElementById("recheckModalBs");
        if (modalElement) {
            modalElement.style.display = "none";
            modalElement.classList.remove("show");
            document.body.classList.remove("modal-open");
        }
    }
}

// 3. Confirm & Send Request
function confirmRecheck() {
    // UI: Disable button to prevent double clicks
    const confirmBtn = document.querySelector('#recheckModalBs .btn[onclick="confirmRecheck()"]');
    const originalText = confirmBtn ? confirmBtn.innerText : "Yes, Recheck";

    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerText = "Sending...";
    }

    // Prepare Data
    const formData = new URLSearchParams();
    formData.append("id", currentId);
    // Send a default message since input was removed, just to satisfy backend if it expects a string
    formData.append("message", "Recheck requested via confirmation popup");

    fetch("/UserMenu/RecheckConsumptions", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
        },
        body: formData.toString()
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                // --- Success: Update UI ---

                // Update Status Badge
                const statusCell = document.getElementById("status-" + currentId);
                if (statusCell) {
                    statusCell.innerHTML = `
                    <span class="status-badge status-recheck">
                        <i class="fa fa-exclamation-circle"></i> Recheck Sent
                    </span>`;
                }

                // Update Action Column
                const actionCell = document.getElementById("action-" + currentId);
                if (actionCell) {
                    actionCell.innerHTML = `<small class="text-muted fst-italic">Pending Review</small>`;
                }

                closeRecheckModal();

                // Success Alert (SweetAlert)
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'success',
                        title: 'Request Sent',
                        text: 'Your recheck request has been submitted successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    });
                }

            } else {
                // Error handling
                if (typeof Swal !== 'undefined') {
                    Swal.fire('Error', 'Failed to send request. Please try again.', 'error');
                } else {
                    alert("Failed to send request.");
                }
            }
        })
        .catch(err => {
            console.error("Fetch error:", err);
            if (typeof Swal !== 'undefined') {
                Swal.fire('Network Error', 'Connection failed.', 'error');
            } else {
                alert("Network error occurred.");
            }
        })
        .finally(() => {
            // Reset button state
            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerText = originalText;
            }
        });
}