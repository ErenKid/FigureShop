function deleteNotification(index) {
    fetch('/Notification/Delete/' + index, { method: 'POST' })
        .then(res => {
            if (res.ok) {
                document.getElementById('notification-item-' + index).remove();
            }
        });
}
