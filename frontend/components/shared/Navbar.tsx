import React, { useEffect, useState } from "react";
import Link from "next/link";
import { useSession, signOut } from "next-auth/react";
import { Profile } from "next-auth";
import {
  Box,
  Flex,
  Avatar,
  IconButton,
  Button,
  Menu,
  MenuButton,
  MenuList,
  MenuItem,
  MenuDivider,
  MenuGroup,
  useColorModeValue,
  useColorMode,
  Image,
  Input,
  useBreakpointValue,
  Icon,
} from "@chakra-ui/react";
import {
  MoonIcon,
  SunIcon,
  SearchIcon,
  AddIcon,
  HamburgerIcon,
} from "@chakra-ui/icons";
import LogoWhite from "../../public/logo_white.svg";
import LogoBlack from "../../public/logo_black.svg";
import AuthHelper from "../../helpers/AuthHelper";

export default function Navbar() {
  const loginPage = "/auth/Login";
  const indexPage = "/";
  const registerPage = "/auth/Signup";
  const isLoggedIn = AuthHelper.isLoggedIn();
  const { data: session, status } = useSession();
  const { colorMode, toggleColorMode } = useColorMode();
  const isMobile = useBreakpointValue({ base: true, md: false });
  const [searchValue, setSearchValue] = useState("");
  const handleSearchChange = (event) => setSearchValue(event.target.value);
  const handleSearchSubmit = () => console.log("Search Value:", searchValue);

  const [user, setUser] = useState({
    id: "",
    email: "",
    username: "",
    avatar: "",
  });
  const [isUserLoggedIn, setIsUserLoggedIn] = useState(false); // New state to track login status
  const [isUserSet, setIsUserSet] = useState(false);

  useEffect(() => {
    // Custom User logged in
    if (isLoggedIn && !isUserSet) {
      AuthHelper.getUser().then((response) => {
        setUser(response);
        setIsUserLoggedIn(true); // Set login status to true
        setIsUserSet(true);
      });
    }
    // Google User logged in
    else if (session && !isLoggedIn) {
      AuthHelper.loginGoogleSSO(
        session.user.email,
        session.user.name,
        (session as any).id,
        session.user.image,
      ).then(() => {
        AuthHelper.getUser().then((response) => {
          setUser(response);
          setIsUserLoggedIn(true); // Set login status to true
          setIsUserSet(true);
        });
      });
    }
  }, [session, isLoggedIn]);

  /**
   * Logs the user out of the application.
   */
  const handleLogOut = async () => {
    AuthHelper.logout();
    // User logged in via Google, so use next-auth's signOut
    if (session) await signOut();

    // Set Logged In Status to false and redirect to index page
    setIsUserLoggedIn(false);
    setIsUserSet(false);
    window.location.href = indexPage;
  };

  const UserProfileMenu = () => (
    <Menu>
      <MenuButton
        aria-label="loggedInMenu"
        as={Button}
        rounded={"full"}
        variant={"link"}
        cursor={"pointer"}
      >
        {user.avatar === "" ? (
          <Avatar
            size={"sm"}
            src={
              "https://images.unsplash.com/photo-1495462911434-be47104d70fa?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1470&q=80"
            }
            boxShadow="0px 0px 10px rgba(0, 0, 0, 0.2)"
            bg="rgba(255, 255, 255, 0.2)"
            backdropFilter="blur(10px)"
          />
        ) : (
          <Avatar
            size={"sm"}
            src={user.avatar}
            boxShadow="0px 0px 10px rgba(0, 0, 0, 0.2)"
            bg="rgba(255, 255, 255, 0.2)"
            backdropFilter="blur(10px)"
          />
        )}
      </MenuButton>
      <MenuList>
        <MenuItem onClick={toggleColorMode}>
          {colorMode === "light"
            ? "Switch to Dark Mode"
            : "Switch to Light Mode"}
        </MenuItem>
        <MenuDivider />
        <MenuGroup>
          <MenuItem>{user.username}</MenuItem>
          <MenuDivider />
          <MenuItem
            onClick={() => (window.location.href = "/profile/MyProfile")}
          >
            My Account
          </MenuItem>
          <MenuItem
            onClick={() => (window.location.href = "/profile/MyPodcast")}
          >
            My Podcast
          </MenuItem>
          <MenuItem>Payments</MenuItem>
        </MenuGroup>
        <MenuDivider />
        <MenuGroup title="Help">
          <MenuItem>Docs</MenuItem>
          <MenuItem>FAQ</MenuItem>
          <MenuDivider />

          <MenuItem
            onClick={handleLogOut}
            style={{ color: "red", fontWeight: "bold" }}
          >
            Logout
          </MenuItem>
        </MenuGroup>
      </MenuList>
    </Menu>
  );

  const LoggedOutMenu = () => (
    <Menu>
      <MenuButton
        menu-id="menuBtn"
        aria-label="Menu"
        as={Button}
        variant={"link"}
        cursor={"pointer"}
      >
        <HamburgerIcon />
      </MenuButton>
      <MenuList>
        <MenuItem
          id="loginBtn"
          onClick={() => (window.location.href = loginPage)}
        >
          Login
        </MenuItem>
        <MenuDivider />
        <MenuItem onClick={() => (window.location.href = registerPage)}>
          Register
        </MenuItem>
      </MenuList>
    </Menu>
  );

  return (
    <>
    <Box
      bg={useColorModeValue("rgba(255, 255, 255, 0.3)", "rgba(0, 0, 0, 0.3)")}
      backdropFilter="blur(35px)"
      p={6}
      mr={"2em"}
      ml={"2em"}
      mb={"3em"}
      position="sticky"
      top={5}
      zIndex={999}
      borderRadius={"95px"}
      boxShadow="0px 0px 15px rgba(0, 0, 0, 0.4)"
      data-testid="navbar-component"
      // semi transparent white outline
      border="3px solid rgba(255, 255, 255, 0.05)"
    >
      <Flex alignItems={"center"} justifyContent={"space-between"} px={6}>
        <Link href="/Main">
          <Box maxWidth={"1.5em"} ml={-2}>
    
            <Image
              src={colorMode === "dark" ? LogoWhite.src : LogoBlack.src}
              alt="logo"
            />
          </Box>
        </Link>
        {isMobile ? (
          <Flex alignItems={"center"}>
            <IconButton
              aria-label="Search"
              icon={<SearchIcon />}
              onClick={handleSearchSubmit}
              variant="ghost"
              size="md"
              rounded={"full"}
              opacity={0.7}
              mr={4}
            />
            <UserProfileMenu />
          </Flex>
        ) : (
          <Flex
            alignItems={"center"}
            as="form"
            onSubmit={(e) => {
              e.preventDefault();
              handleSearchSubmit();
            }}
          >
            <Input
              placeholder="Search"
              size="sm"
              borderRadius="full"
              mr={4}
              value={searchValue}
              onChange={handleSearchChange}
            />
            <Link href="/Create">
              <IconButton
                aria-label="Create"
                icon={<AddIcon />}
                variant="ghost"
                size="md"
                rounded={"full"}
                opacity={0.7}
                mr={4}
                color={colorMode === "dark" ? "white" : "black"}
              />
            </Link>
            {isUserLoggedIn ? <UserProfileMenu /> : <LoggedOutMenu />}
          </Flex>
        )}
      </Flex>
    </Box>
    </>
  );
}
