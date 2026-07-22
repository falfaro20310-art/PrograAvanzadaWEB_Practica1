// Validaciones de la vista de perfil

document.addEventListener('DOMContentLoaded', function () {

    const password = document.getElementById('profilePassword');
    const confirmPassword = document.getElementById('profileConfirmPassword');

    if (!password || !confirmPassword) return;

    // Marca la confirmacion como invalida cuando no coincide con la contrasena
    function validatePasswordMatch() {
        confirmPassword.setCustomValidity(
            confirmPassword.value !== password.value ? 'no-coincide' : ''
        );
    }

    password.addEventListener('input', validatePasswordMatch);
    confirmPassword.addEventListener('input', validatePasswordMatch);

});
