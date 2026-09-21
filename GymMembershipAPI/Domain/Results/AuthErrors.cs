namespace GymMembershipAPI.Domain.Results;

public class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "AuthErrors.InvalidCredentials",
        "El correo electrónico o la contraseña son incorrectos."
    );

    public static readonly Error UserNotFound = new(
        "AuthErrors.UserNotFound",
        "No existe un usuario registrado con este correo."
    );

    public static readonly Error UserAlreadyExists = new(
        "AuthErrors.UserAlreadyExists",
        "Ya existe un usuario registrado con este correo electrónico."
    );
}