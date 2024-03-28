import React, { useState, FormEvent, useEffect } from "react";
import { Box, Button, Input, Stack, Text, Flex, ButtonGroup, Img, InputGroup, InputRightElement, FormControl, IconButton, Alert, AlertDescription, Container } from "@chakra-ui/react";
import AuthHelper from "../../helpers/AuthHelper";
import { LoginRequest } from "../../types/Requests";
import { FaGoogle } from "react-icons/fa";
import { signIn } from "next-auth/react";
import { ViewIcon, ViewOffIcon } from "@chakra-ui/icons";
import Logo from "../../public/logos/logo_white.svg";

interface LoginProps {
  infoMessage?: string;
}

const Login: React.FC<LoginProps> = ({ infoMessage = "Get into the new age of podcasting." }) => {

  const mainPage = "/";
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loginError, setLoginError] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [loginSuccessful, setLoginSuccessful] = useState(false);

  useEffect(() => {
    AuthHelper.authMeRequest().then((res) => {
      if (res.status === 200) {
        window.location.href = mainPage;
      }
    });
  }, []);

  const handlePasswordVisibility = () => setShowPassword(!showPassword);

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    console.log("Login Clicked");
    const loginRequest: LoginRequest = {
      email: email,
      password: password,
    };

    const response = await AuthHelper.authLoginRequest(loginRequest);
    if (response.status === 200) {
      AuthHelper.authMeRequest().then((res) => {
        if (res.status === 200) {
          if (window) window.sessionStorage.setItem("userInfo", JSON.stringify(res.userMenuInfo));
        }
      });

      // Set loginSuccessful to true
      setLoginSuccessful(true);
      window.location.href = mainPage;
    } else {
      setLoginError(response.data);
    }
  };

  return (
    <>
      <Container variant={"authBox"} bg="az.blackish" style={{ outline: 'none', fontFamily: "'Neue Montreal'" }}>
        {loginError && (
          <Alert status="error" borderRadius="xl" mb={4} p={2}>
            <AlertDescription display="block" fontSize="sm">
              {loginError}
            </AlertDescription>
          </Alert>
        )}
        <Flex justifyContent="center" mb={4}>
          <Img src={Logo.src} alt="logo" style={{ maxWidth: "40px" }} />
        </Flex>
        <form onSubmit={handleLogin}>
          <Stack spacing={4}>
            <Input type="text" id="email" placeholder="Enter Email/Username" borderRadius="2xl" bg="whiteAlpha.200" borderColor="whiteAlpha.400" _placeholder={{ color: "whiteAlpha.700" }} required onChange={(e) => setEmail(e.target.value)} />
            <FormControl>
              <InputGroup>
                <Input
                  id="password"
                  type={showPassword ? "text" : "password"}
                  placeholder="Enter Password"
                  borderRadius="2xl"
                  bg="whiteAlpha.200"
                  borderColor="whiteAlpha.400"
                  _placeholder={{ color: "whiteAlpha.700" }}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <InputRightElement>
                  <IconButton bg="transparent" variant="ghost" aria-label={showPassword ? "Hide password" : "Show password"} icon={showPassword ? <ViewOffIcon /> : <ViewIcon />} onClick={handlePasswordVisibility} borderRadius="3xl" />
                </InputRightElement>
              </InputGroup>
            </FormControl>
            <Text color="brand.300" fontSize="sm" mb={1} align={"right"}>
              <Box as="a" href="/auth/ForgotPassword" color={"az.blue"} fontWeight="semibold">
                Forgot password ?
              </Box>
            </Text>
            <Button id="loginBtn" type="submit" color="az.blackish" bg="az.offWhite" size="md" fontSize="md" borderRadius="2xl" py={3} boxShadow="0 2px 4px rgba(0, 0, 0, 0.2)">
              Log in
            </Button>


            <Button w="100%" leftIcon={<FaGoogle />} onClick={() => signIn("google")} color="az.greyish" bg="az.offWhite" fontSize="md" mb={3} borderRadius="2xl">
              Continue with Google
            </Button>

          </Stack>

          {/* <Text color="gray.400" fontSize="sm" align={"center"}>
            Don't have an account?{" "}
            <Box as="a" href="/auth/Signup" color="brand.100" fontWeight="semibold">
              Sign up
            </Box>
          </Text> */}
        </form>
      </Container>
    </>
  );
};

export default Login;
