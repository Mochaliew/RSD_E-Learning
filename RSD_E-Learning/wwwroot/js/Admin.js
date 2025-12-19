document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.addEventListener('click', function () {

        const id = this.dataset.id;
        const row = this.closest('tr');
        const badge = row.querySelector('.status-badge');
        const button = this;

        fetch('/AdminPromoCode/ToggleAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken':
                    document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ id: parseInt(id) })
        })
            .then(res => res.json())
            .then(data => {
                if (!data.success) return;

                if (data.isActive) {
                    badge.className = 'badge bg-success status-badge';
                    badge.innerText = 'Active';

                    button.className = 'btn btn-sm btn-outline-danger toggle-btn fixed-btn';
                    button.innerText = 'Deactivate';
                } else {
                    badge.className = 'badge bg-secondary status-badge';
                    badge.innerText = 'Inactive';

                    button.className = 'btn btn-sm btn-outline-success toggle-btn fixed-btn';
                    button.innerText = 'Activate';
                }
            });
    });
});
