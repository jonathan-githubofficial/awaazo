// src/Login.tsx
import React, { useState, FormEvent, useEffect } from "react";
import {
  Box,
  Button,
  FormControl,
  FormLabel,
  Input,
  Stack,
  Text,
} from "@chakra-ui/react";

import logo from "../styles/images/logo.png";
import LogoWhite from "../../public/logo_white.svg";
import Image from "next/image";
import AuthHelper from "../../helpers/AuthHelper";
import { RedirectType, redirect } from "next/navigation";
import { useSession, signIn, signOut } from "next-auth/react"
import { useRouter } from 'next/router';


const Login: React.FC = () => {
  // CONSTANTS
  const mainPage = "/Main";

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loginError, setLoginError] = useState<string | null>(null);
  const { data: session } = useSession()
  const router = useRouter(); // To store login error

  useEffect(() => {
    // Check if user is already logged in
    if (AuthHelper.isLoggedIn()) {
      window.location.href = mainPage;
    }
  }, []);

  const handleLogin = async (e: FormEvent) => {

    e.preventDefault(); // Prevent default form submission
    const response = await AuthHelper.login(email, password);
    console.log(response);
    if (response) {
      window.location.href = mainPage;
    } else {
      setLoginError("Login failed. Invalid Email and/or Password.");
    }
  };

  return (
    <>
        <Box
          p={6}
          display="flex" // Use flexbox to center vertically
          flexDirection="column" // Stack children vertically
          justifyContent="center" // Center vertically
          alignItems="center" // Center horizontally
        >
          <img
            src={LogoWhite.src}
            alt="logo"
            style={{
              maxWidth: "20em",
            }}
          />
          <Text
            style={{
              fontSize: "2.5rem",
              fontWeight: "bold",
              textAlign: "center",
              marginBottom: "1rem",
            }}
          >
            Welcome Back
          </Text>
          <Text
            style={{
              fontSize: "1.3rem",
              textAlign: "center",
              marginBottom: "3rem",
            }}
          >
            Sign in to continue
          </Text>
          <Button type="submit" colorScheme="green" size="lg" fontSize="md" onClick={() => signIn('google')} marginBottom={5} p={6}>
            Sign in with Google
          </Button>
          <Text
            style={{
              fontSize: "1.3rem",
              textAlign: "center",
              marginBottom: "1rem",
            }}
          >
            or
          </Text>
          {loginError && <Text color="red.500">{loginError}</Text>}
          <form onSubmit={handleLogin}>
            <Stack spacing={4}>
              <FormControl>
                <FormLabel>Email address</FormLabel>
                <Input
                  type="email"
                  id="email"
                  placeholder="Enter your email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </FormControl>

              <FormControl>
                <FormLabel>Password</FormLabel>
                <Input
                  id="password"
                  type="password"
                  placeholder="Enter your password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </FormControl>

              <Button id="loginBtn" type="submit" colorScheme="gray" size="lg" fontSize="md">
                Login
              </Button>
              <Text>
                Don&apos;t have an account?{" "}
                <a href="/auth/Signup" style={{ color: "#3182CE" }}>
                  Sign up
                </a>
              </Text>
              <div className="sso-group">
                <Stack
                  direction="column"
                  style={{ alignSelf: "center" }}
                  gap={4}
                ></Stack>
              </div>
            </Stack>
          </form>
        </Box>
    </>
  );
};

export default Login;