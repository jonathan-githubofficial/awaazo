import React, { FormEvent, useState } from "react";
import { Box, Button, FormControl, FormLabel, Input, Stack, Text, Flex, ButtonGroup, Img } from "@chakra-ui/react";
import Logo from "../../public/logo_white.svg";
import { signIn } from "next-auth/react";
import AuthHelper from "../../helpers/AuthHelper";
import { useSession } from "next-auth/react";
import { useEffect } from "react";
import { RegisterRequest } from "../../utilities/Requests";
import { FaGoogle } from "react-icons/fa";
import { ViewIcon, ViewOffIcon } from "@chakra-ui/icons";

const SignUp: React.FC = () => {
  const setupPage = "/profile/ProfileSetup";
  const [email, setEmail] = useState<string | null>("");
  const [username, setUsername] = useState<string | null>("");
  const [usernameCharacterCount, setUsernameCharacterCount] = useState<number>(0);
  const [password, setPassword] = useState<string | null>("");
  const [confirmPassword, setConfirmPassword] = useState<string | null>("");
  const [dateOfBirth, setDateOfBirth] = useState<string | null>("");
  const [signUpError, setSignUpError] = useState<string | null>("");
  const { data: session } = useSession();
  const [googleSignUpClicked, setGoogleSignUpClicked] = useState(false);

  useEffect(() => {}, [session, googleSignUpClicked]);

  const handleGoogleSignUp = async () => {
    setGoogleSignUpClicked(true);
    signIn("google");
  };

  const handleUsernameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newUsername = e.target.value.slice(0, 25);
    setUsername(newUsername);
    setUsernameCharacterCount(newUsername.length);
  };

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!(password === confirmPassword)) {
      setSignUpError("Passwords do not match.");
      console.debug(signUpError);
    } else {
      const registerRequest: RegisterRequest = {
        email: email,
        password: password,
        username: username,
        dateOfBirth: dateOfBirth,
        gender: "None",
      };

      const response = await AuthHelper.authRegisterRequest(registerRequest);

      if (response.status === 200) {
        window.location.href = setupPage;
      } else {
        setSignUpError(response.data);
      }
    }
  };

  return (
    <Flex minHeight="100vh" align="center" justify="center">
      <Box
        p={6}
        bg={"rgba(0, 0, 0, 0.3)"}
        border="3px solid rgba(255, 255, 255, 0.05)"
        backdropFilter="blur(10px)"
        boxShadow="0 4px 6px rgba(0, 0, 0, 0.2)"
        borderRadius="3xl"
        maxW="400px"
        w="full"
        textAlign="center"
      >
        <Flex justifyContent="center" mb={4}>
          <Img src={Logo.src} alt="logo" style={{ maxWidth: "40px" }} />
        </Flex>
        <Text fontSize="lg" fontWeight="bold" color="white" mb={1} align={"left"} textColor={"brand.100"}>
          Sign Up to Amazon
        </Text>
        <Text fontSize="sm" color="gray.400" mb={6} align={"left"}>
          Join Awaazo and transform podcasting with AI brilliance Create, Connect, Captivate!
        </Text>

        {signUpError && <Text color="red.500">{signUpError}</Text>}
        <form onSubmit={handleSignUp}>
          <Stack spacing={3}>
            <FormControl>
              <Input type="email" id="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} required borderRadius="2xl" />
            </FormControl>
            <FormControl>
              <Input type="text" id="username" placeholder="Username" value={username} onChange={handleUsernameChange} required borderRadius="2xl" pr="50px" />
              <Text position="absolute" right="9px" bottom="9px" fontSize="sm" color="gray.500">
                {usernameCharacterCount}/25
              </Text>
            </FormControl>
            <FormControl>
              <Input type="password" id="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} required borderRadius="2xl" />
            </FormControl>
            <FormControl>
              <Input type="password" id="confirmPassword" placeholder="Confirm Password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required borderRadius="2xl" />
            </FormControl>
            <FormControl mt={2} mb={2}>
              <FormLabel htmlFor="date" fontWeight="normal" fontSize="sm" ml={2}>
                Date of Birth
              </FormLabel>
              <Input type="date" id="date" value={dateOfBirth} onChange={(e) => setDateOfBirth(e.target.value)} required borderRadius="2xl" />
            </FormControl>

            <Button id="loginBtn" type="submit" color="black" bg="brand.100" size="md" fontSize="md" borderRadius="2xl" py={3} boxShadow="0 2px 4px rgba(0, 0, 0, 0.2)">
              Sign Up
            </Button>

            <Flex alignItems="center" justifyContent="center">
              <Box flex="1" height="1px" bg="whiteAlpha.400" />
              <Text fontSize="sm" mx={2} color="gray.500">
                Or authorize with
              </Text>
              <Box flex="1" height="1px" bg="whiteAlpha.400" />
            </Flex>

            <ButtonGroup isAttached justifyContent="center">
              <Button leftIcon={<FaGoogle />} onClick={handleGoogleSignUp} size="md" mb={3} borderRadius="2xl">
                Google
              </Button>
            </ButtonGroup>

            <Text color="gray.400" fontSize="sm" align={"center"}>
              Already have an account?{" "}
              <Box as="a" href="/auth/Login" color="brand.100" fontWeight="semibold">
                Log in
              </Box>
            </Text>
          </Stack>
        </form>
      </Box>
    </Flex>
  );
};

export default SignUp;
