
document.addEventListener('DOMContentLoaded', function () {
    const btnAddPost = document.getElementById('btnAddPost');
    const addPostModal = document.getElementById('addPostModal');
    const btnCloseAddPostModal = document.getElementById('btnCloseAddPostModal');
    const btnCancelAddPost = document.getElementById('btnCancelAddPost');

    if (btnAddPost && addPostModal) {
        btnAddPost.addEventListener('click', () => {
            document.querySelector('input[name="LocationRawId"]').value = '@Model.SelectedLocationRawId';
            addPostModal.classList.remove('hidden');
            addPostModal.classList.add('flex');
        });
    }

    function closeModal() {
        addPostModal.classList.add('hidden');
        addPostModal.classList.remove('flex');
    }

    btnCloseAddPostModal.addEventListener('click', closeModal);
    btnCancelAddPost.addEventListener('click', closeModal);

    // Nếu muốn đóng modal khi click ra ngoài phần nội dung modal
    addPostModal.addEventListener('click', (e) => {
        if (e.target === addPostModal) {
            closeModal();
        }
    });
});