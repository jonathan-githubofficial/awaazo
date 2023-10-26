import axios from "axios";
import { BaseResponse, LoginResponse, LogoutResponse, MeResponse, RegisterResponse } from "../utilities/Responses";
import EndpointHelper from "./EndpointHelper";
import RestHelper, { UserInfo } from "./RestHelper";
import { LoginRequest, RegisterRequest } from "../utilities/Requests";
import { data } from "cypress/types/jquery";
import { UserMenuInfo } from "../utilities/Interfaces";



/**
 * Handles the Authentication process with the Backend.
 */
export default class AuthHelper{

    /**
     * Logs the user out of the application.
     * @returns A BaseResponse object with the server's response.
     */
    public static authLogoutRequest = async () : Promise<LogoutResponse> => {

        if(window){
            window.sessionStorage.removeItem("userInfo")
        }

        // Create the request options.
        const options =
        {
            method: 'GET',
            url: EndpointHelper.getAuthLogoutEndpoint(),
            headers:
            {
                accept: '*/*',
            },
            withCredentials: true
        }

        try {
            console.debug("Sending the following authLogoutRequest...");
            console.debug(options);

            // Send the request and wait for the response.
            const requestResponse = await axios(options);

            console.debug("Received the following authLogoutResponse...");
            console.debug(requestResponse);

            // Return the response
            return {
                status: requestResponse.status,
                message: requestResponse.statusText
            }
        }
        catch(error) {
            // Return the error.
            return {
                status: error.response.status,
                message: error.response.statusText
            }
        }
    }

    
    public static authLoginRequest = async (requestData: LoginRequest) : Promise<LoginResponse> => {
        // Create the request options.
        const options =
        {
            method: 'POST',
            url: EndpointHelper.getAuthLoginEndpoint(),
            data: requestData,
            headers:
            {
                accept: '*/*',
                "Content-Type": "application/json"
            },
            withCredentials: true
        }

        try{
            console.debug("Sending the following authLoginRequest...");
            console.debug(options);

            // Send the request and wait for the response.
            const requestResponse = await axios(options);

            console.debug("Received the following authLoginResponse...");
            console.debug(requestResponse);

            // Return the response
            return {
                status: requestResponse.status,
                message: requestResponse.statusText,
                data: requestResponse.data
            }
        }
        catch(error){
            console.log(error)
            return {
                status: error.response.status,
                message: error.response.statusText,
                data: error.response.data
            }
        }
    }

    public static authRegisterRequest = async (requestData: RegisterRequest) : Promise<RegisterResponse> => {
        // Create the request options.
        const options =
        {
            method: 'POST',
            url: EndpointHelper.getAuthRegisterEndpoint(),
            data: requestData,
            headers:
            {
                accept: '*/*',
                "Content-Type": "application/json"
            },
            withCredentials: true
        }

        try{
            console.debug("Sending the following authRegisterRequest...");
            console.debug(options);

            // Send the request and wait for the response.
            const requestResponse = await axios(options);

            console.debug("Received the following authRegisterResponse...");
            console.debug(requestResponse);

            // Return the response
            return {
                status: requestResponse.status,
                message: requestResponse.statusText,
                data: requestResponse.data
            }
        }
        catch(error){
            return {
                status: error.response.status,
                message: error.response.statusText,
                data: error.response.data
            }
        }
    }

    public static authMeRequest = async () : Promise<MeResponse> => {
        // Create the request options.
        const options =
        {
            method: 'GET',
            url: EndpointHelper.getAuthMeEndpoint(),
            headers:
            {
                accept: '*/*'
            },
            withCredentials: true
        }

        try{
            console.debug("Sending the following authMeRequest...");
            console.debug(options);

            // Send the request and wait for the response.
            const requestResponse = await axios(options);

            console.debug("Received the following authMeResponse...");
            console.debug(requestResponse);

            if(window){
                window.sessionStorage.setItem("userInfo",JSON.stringify(requestResponse.data))
            }

            // Return the response
            return {
                status: requestResponse.status,
                message: requestResponse.statusText,
                userMenuInfo: requestResponse.data
            }
        }
        catch(error){

            if(window){
                window.sessionStorage.removeItem("userInfo")
            }

            return {
                status: error.response.status,
                message: error.response.statusText,
                userMenuInfo: null
            }
        }
    }

    public static isLoggedIn = async (): Promise<boolean> => {
        return await this.authMeRequest().then(res => res.status === 200)
    }

    public static loginGoogleSSO = async (email:string,username:string,sub:string,avatar:string): Promise<boolean> =>
    {
        // Send the request.
        const res = await RestHelper.authGoogleSSORequest(email,username,sub,avatar);

        console.debug(res);
        
        // Successful login.
        if(res.status === 200){
            console.debug("Login Successful")
            this.setToken(res.data.token);
            return true;
        }
        // Unsuccessful login.
        else{
            console.error("Login Failed")
            return false;
        }
        
    }

}