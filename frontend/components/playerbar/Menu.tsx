// PlaylistMenu.tsx
import React, { useState, useEffect } from "react";
import {
  Box,
  Menu,
  MenuButton,
  IconButton,
  Input,
  Textarea,
  MenuList,
  MenuItem,
  MenuDivider,
  useToast,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  Button,
  useDisclosure,
  FormControl,
  FormLabel,
} from "@chakra-ui/react";
import { BsFillSkipForwardFill } from "react-icons/bs";
import { MdIosShare, MdOutlinePlaylistAdd, MdOutlinePodcasts } from "react-icons/md";

import Link from "next/link";

import { usePlayer } from "../../utilities/PlayerContext";
import PlaylistHelper from "../../helpers/PlaylistHelper";
import { PlaylistEditRequest } from "../../types/Requests";
import { useRouter } from "next/router";
import { CgPlayList, CgPlayListSearch } from "react-icons/cg";
import { FaDeleteLeft } from "react-icons/fa6";
import ShareComponent from "../social/Share";
import { CiMenuKebab } from "react-icons/ci";
import ViewQueueModal from "../playlist/ViewQueueModal";
import CreatePlaylistModal from "../playlist/CreatePlaylistModal";
import AddToPlaylistModal from "../playlist/AddToPlaylistModal";
import AuthHelper from "../../helpers/AuthHelper";
import LoginPrompt from "../auth/LoginPrompt";

const PlayerMenu = ({ episode }) => {
  const { dispatch } = usePlayer();

  const { onOpen, onClose, isOpen } = useDisclosure();

  const [isShareModalOpen, setIsShareModalOpen] = useState(false);
  const onShareModalClose = () => setIsShareModalOpen(false);
  const onShareModalOpen = () => setIsShareModalOpen(true);

  const [isQueueModalOpen, setIsQueueModalOpen] = useState(false);
  const onQueueModalClose = () => setIsQueueModalOpen(false);
  const onQueueModalOpen = () => setIsQueueModalOpen(true);

  const [isAddToPlaylistModalOpen, setIsAddToPlaylistModalOpen] = useState(false);
  const onAddToPlaylistModalClose = () => setIsAddToPlaylistModalOpen(false);
  const onAddToPlaylistModalOpen = () => setIsAddToPlaylistModalOpen(true);

  // State to track whether the menu is open or not
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const handleMenuToggle = () => setIsMenuOpen(!isMenuOpen);

  const [showLoginPrompt, setShowLoginPrompt] = useState(false);

  const handleAddPlaylistClick = () => {
    console.log("Clicked");
    // Check login status before opening the modal
    AuthHelper.authMeRequest().then((response) => {
      if (response.status === 401) {
        console.log("User not logged in");
        // Handle not logged in state (e.g., show a login prompt)
        onAddToPlaylistModalClose();
        setShowLoginPrompt(true);
        return;
      } else {
        // User is logged in, open the modal
        onAddToPlaylistModalOpen();
      }
    });
  };

  const handleRemoveFromQueue = () => {
    dispatch({ type: "REMOVE_FROM_QUEUE", payload: episode });
  };

  return (
    <Box style={{ position: "relative", zIndex: 9999 }} data-cy={`3-dots`}>
      <Menu isOpen={isMenuOpen} onClose={handleMenuToggle}>
        <MenuButton as={IconButton} aria-label="Options" icon={<CiMenuKebab />} variant="ghost" fontSize="20px" ml={1} _hover={{ boxShadow: "lg" }} onClick={handleMenuToggle} />
        <MenuList
          style={{
            backgroundColor: "rgba(50, 50, 50, 0.8)",
            backdropFilter: "blur(4px)",
          }}
        >
          <MenuItem
            _hover={{
              backgroundColor: "rgba(255, 255, 255, 0.8)",
              fontWeight: "bold",
            }}
            style={{
              backgroundColor: "transparent",
            }}
            onClick={() => {handleAddPlaylistClick() ; onAddToPlaylistModalOpen(); }}
          >
            Add to Playlist
            <MdOutlinePlaylistAdd size="20px" style={{ marginLeft: "auto", color: "white" }} />
          </MenuItem>
          <MenuDivider />
          <MenuItem
            _hover={{
              backgroundColor: "rgba(255, 255, 255, 0.8)",
              fontWeight: "bold",
            }}
            style={{
              backgroundColor: "transparent",
            }}
            onClick={onQueueModalOpen}
          >
            View Queue
            <CgPlayListSearch size="18px" style={{ marginLeft: "auto", color: "white" }} />
          </MenuItem>
          <MenuItem
            _hover={{
              backgroundColor: "rgba(255, 255, 255, 0.8)",
              fontWeight: "bold",
            }}
            style={{
              backgroundColor: "transparent",
            }}
            onClick={handleRemoveFromQueue}
          >
            Remove from Queue
            <FaDeleteLeft size="18px" style={{ marginLeft: "auto", color: "white" }} />
          </MenuItem>
          <Link href={"Explore/" + episode?.podcastId} style={{ textDecoration: "none" }}>
            <MenuItem
              _hover={{
                backgroundColor: "rgba(255, 255, 255, 0.8)",
                fontWeight: "bold",
              }}
              style={{
                backgroundColor: "transparent",
              }}
            >
              Go to Podcast Page
              <MdOutlinePodcasts size="18px" style={{ marginLeft: "auto", color: "white" }} />
            </MenuItem>
          </Link>

          <MenuDivider />
          <MenuItem
            onClick={onShareModalOpen}
            _hover={{
              backgroundColor: "rgba(255, 255, 255, 0.8)",
              fontWeight: "bold",
            }}
            style={{
              backgroundColor: "transparent",
            }}
          >
            Share <MdIosShare size="20px" style={{ marginLeft: "auto", color: "white" }} />
          </MenuItem>
        </MenuList>
      </Menu>
      <Modal isOpen={isShareModalOpen} onClose={onShareModalClose}>
        <ModalOverlay />
        <ModalContent>
          <ModalHeader>Share this Episode</ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <ShareComponent content={episode} contentType="episode" />
          </ModalBody>
        </ModalContent>
      </Modal>
      <ViewQueueModal isOpen={isQueueModalOpen} onClose={onQueueModalClose} />
      <AddToPlaylistModal episode={episode} isOpen={isAddToPlaylistModalOpen} onClose={onAddToPlaylistModalClose} />
      {/* LoginPrompt */}
      {showLoginPrompt && (
          <LoginPrompt
            isOpen={showLoginPrompt}
            onClose={() => setShowLoginPrompt(false)}
            infoMessage="To add this episode to your playlist, you must be logged in. Please log in or create an account."
          />
        )}
      </Box>
  );
};

export default PlayerMenu;
