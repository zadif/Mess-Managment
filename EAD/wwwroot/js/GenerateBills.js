    // 1. Toggle Details Function
        function toggleDetails(userId) {
        const el = document.getElementById(`details-${userId}`);
        if (el.style.display === "block") {
            el.style.display = "none";
        } else {
            el.style.display = "block";
        }
    }

        // 2. Custom Toast Notification
        function showToast(message, type = 'success') {
        const container = document.getElementById('toastContainer');
        const toast = document.createElement('div');
        toast.className = `custom-toast ${type === 'success' ? 'toast-success' : 'toast-error'}`;

        let icon = type === 'success' ? '<i class="fas fa-check-circle"></i>' : '<i class="fas fa-exclamation-circle"></i>';

        toast.innerHTML = `${icon} <span>${message}</span>`;
        container.appendChild(toast);

        // Remove after 3 seconds
        setTimeout(() => {
            toast.style.animation = "fadeOut 0.3s forwards";
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    // 3. Single Bill Generation
    document.querySelectorAll('.btn-generate-single').forEach(btn => {
            btn.addEventListener('click', function () {
                const userId = this.dataset.userid;
                const total = this.dataset.total;
                const card = document.getElementById(`card-${userId}`);

                this.disabled = true;
                this.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

                const formData = new FormData();
                formData.append("userId", userId);
                formData.append("total", total);

                fetch("/Admin/GenerateBills", {
                    method: "POST",
                    body: formData
                })
                    .then(r => r.json())
                    .then(data => {
                        if (data.success) {
                            showToast(`Bill generated for User #${userId}`, 'success');
                            // Animate and remove card
                            card.style.opacity = '0';
                            setTimeout(() => card.remove(), 300);
                        } else {
                            showToast('Failed to generate bill.', 'error');
                            this.disabled = false;
                            this.innerHTML = 'Generate Bill';
                        }
                    })
                    .catch(err => {
                        showToast('Network error occurred.', 'error');
                        this.disabled = false;
                        this.innerHTML = 'Generate Bill';
                    });
            });
    });

        // 4. Modal Logic
        const modal = document.getElementById('confirmModal');
        const btnOpen = document.getElementById('btnOpenModal');
        const btnCancel = document.getElementById('btnCancelModal');
        const btnConfirm = document.getElementById('btnConfirmGenerate');

        if(btnOpen) {
            btnOpen.addEventListener('click', () => modal.style.display = 'flex');
        btnCancel.addEventListener('click', () => modal.style.display = 'none');
    }

        // 5. Generate All Logic
        if(btnConfirm) {
            btnConfirm.addEventListener('click', async () => {
                modal.style.display = 'none'; // Close modal

                const buttons = document.querySelectorAll('.btn-generate-single');
                const progressText = document.getElementById('progressText');
                const mainBtn = document.getElementById('btnOpenModal');

                mainBtn.disabled = true;
                mainBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';

                let successCount = 0;
                let failCount = 0;

                for (let i = 0; i < buttons.length; i++) {
                    const btn = buttons[i];
                    const userId = btn.dataset.userid;
                    const total = btn.dataset.total;
                    const card = document.getElementById(`card-${userId}`);

                    progressText.innerHTML = `Processing ${i + 1} of ${buttons.length}...`;

                    const formData = new FormData();
                    formData.append("userId", userId);
                    formData.append("total", total);

                    try {
                        const res = await fetch("/Admin/GenerateBills", {
                            method: "POST",
                            body: formData
                        });
                        const data = await res.json();

                        if (data.success) {
                            successCount++;
                            card.style.opacity = '0.5'; // Dim it to show processed
                        } else {
                            failCount++;
                        }
                    } catch (e) {
                        failCount++;
                    }
                }

                // Final State
                progressText.innerHTML = "";
                showToast(`Complete! ${successCount} generated, ${failCount} failed.`, successCount > 0 ? 'success' : 'error');

                // Reload page after short delay to clear list
                setTimeout(() => location.reload(), 1500);
            });
    }

