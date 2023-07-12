using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthManager : MonoBehaviour
{
    //Firebase
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;

    //Login
    [Header("Login")]
    public TMP_InputField emailLogin;
    public TMP_InputField passwordLogin;
    public TMP_Text warningLogin;
    public TMP_Text confirmLogin;

    //Register
    [Header("Register")]
    public TMP_InputField usernameRegister;
    public TMP_InputField emailRegister;
    public TMP_InputField passwordRegister;
    public TMP_InputField passwordVerifyRegister;
    public TMP_Text warningRegister;

    private void Awake(){
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if(dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }else{
                Debug.LogError("Error en dependencias Firebase: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase(){
        Debug.Log("Se ha agregado autentificación con Firebase");
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton(){
        StartCoroutine(Login(emailLogin.text, passwordLogin.text));
    }
    
    public void RegisterButton(){
        StartCoroutine(Register(emailRegister.text, passwordRegister.text, usernameRegister.text)); 
    }

    private IEnumerator Login(string _email, string _password){
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email,_password);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if(LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Fallo en registrarse la tarea con {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError codigoError = (AuthError)firebaseEx.ErrorCode;

            string mensaje = "Login Fallido";
            switch(codigoError)
            {
                case AuthError.MissingEmail:
                    mensaje = "Agregue el email";
                    break;
                case AuthError.MissingPassword:
                    mensaje = "Agregue la contraseña";
                    break;
                case AuthError.WrongPassword:
                    mensaje = "Contraseña incorrecta";
                    break;
                case AuthError.InvalidEmail:
                    mensaje = "Correo invalido";
                    break;
                case AuthError.UserNotFound:
                    mensaje = "Usuario no registrado";
                    break;
            }
            warningLogin.text = mensaje;
        }
        else
        {
            User = LoginTask.Result.User;
            Debug.LogFormat("El usuario se conectó exitosamente: {0} {1}", User.DisplayName, User.Email);
            warningLogin.text = "";
            confirmLogin.text = "Conectado";
        }   
    }

    private IEnumerator Register(string _email, string _password, string _username){
        if(_username == "")
        {
            warningRegister.text = "Falta agregar usuario";
        }
        else if(passwordRegister.text != passwordVerifyRegister.text){
            warningRegister.text = "Contraseñas no coinciden";
        }
        else{
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Fallo en el registro {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError codigoError = (AuthError)firebaseEx.ErrorCode;

                string mensaje = "Fallo en el registro";
                switch (codigoError)
                {
                    case AuthError.MissingEmail:
                        mensaje = "Agregue email";
                        break;
                    case AuthError.MissingPassword:
                        mensaje = "Agregue la contraseña";
                        break;
                    case AuthError.WeakPassword:
                        mensaje = "Contraseña muy sencilla";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        mensaje = "Email ya registrado";
                        break;
                }
                warningRegister.text = mensaje;
            }
            else
            {
                User = RegisterTask.Result.User;

                if(User != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username};

                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Fallo al registrarse {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError codigoError = (AuthError)firebaseEx.ErrorCode;
                        warningRegister.text = "Fallo al escoger usuario";
                    }
                    else
                    {
                        UIManager.instance.LoginScreen();
                        warningRegister.text = "";
                    }
                }
            }
        }
    }
}
