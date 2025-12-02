    function verifyBill(billId, button) {

        // 1. Show Styled Confirmation Popup
        Swal.fire({
            title: 'Verify Payment?',
            text: "Are you sure you want to mark this bill as verified?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#1bc5bd', // Success Green
            cancelButtonColor: '#f64e60',  // Danger Red
            confirmButtonText: 'Yes, verify it!',
            background: '#1e1e2d',         // Dark Card Background
            color: '#ffffff',              // White Text
            iconColor: '#1bc5bd'           // Green Icon
        }).then((result) => {

            if (result.isConfirmed) {
                // 2. If User Clicked Yes, Proceed

                // Show Loading State on Button
                const originalContent = button.innerHTML;
                button.disabled = true;
                button.innerHTML = "Processing...";

                const formData = new FormData();
                formData.append("billId", billId);

                fetch("/Admin/VerifyBill", {
                    method: "POST",
                    body: formData
                })
                    .then(r => r.json())
                    .then(response => {
                        if (response === 1) {

                            // 3. Show Success Popup (Toast style)
                            Swal.fire({
                                title: 'Verified!',
                                text: 'The payment has been verified successfully.',
                                icon: 'success',
                                background: '#1e1e2d',
                                color: '#ffffff',
                                confirmButtonColor: '#1bc5bd'
                            });

                            // 4. Update UI without reload
                            const row = button.closest("tr");

                            // Update Status Badge
                            row.cells[5].innerHTML = '<span class="status-badge status-verified">Verified</span>';

                            // Update Action Column
                            row.cells[6].innerHTML = `
                                <span class="text-done">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>
                                    Completed
                                </span>`;
                        } else {
                            // Logic Error
                            button.disabled = false;
                            button.innerHTML = originalContent;
                            showError("Could not verify the bill. Please try again.");
                        }
                    })
                    .catch(() => {
                        // Network Error
                        button.disabled = false;
                        button.innerHTML = originalContent;
                        showError("Network connection error.");
                    });
            }
        });
        }

    function showError(message) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: message,
            background: '#1e1e2d',
            color: '#ffffff',
            confirmButtonColor: '#f64e60'
        });
        }
