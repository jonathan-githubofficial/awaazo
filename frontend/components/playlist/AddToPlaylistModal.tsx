import React, { useEffect, useState } from "react";
import {
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalFooter,
  ModalBody,
  ModalCloseButton,
  VStack,
  FormControl,
  Input,
  Button,
  Text,
  Select,
  Box,
} from "@chakra-ui/react";
import NexLink from "next/link";
import PlaylistHelper from "../../helpers/PlaylistHelper";
import { Playlist } from "../../utilities/Interfaces";

const AddToPlaylistModal = ({ isOpen, onClose, episode }) => {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [page, setPage] = useState(0);
  const pageSize = 20;

  const [targetPlaylistId, setTargetPlaylistId] = useState("");

  // Form errors
  const [playlistError, setPlaylistError] = useState("");

  useEffect(() => {
    PlaylistHelper.playlistMyPlaylistsGet(page, pageSize).then((res2) => {
      if (res2.status == 200) {
        setPlaylists(res2.playlists);
      } else {
        setPlaylistError("Podcasts cannot be fetched");
      }
    });
  }, [page]);

  const userPlaylists = playlists.filter(
    (playlist) => playlist.isHandledByUser,
  );

  const handleAddToExistingPlaylist = async () => {
    // Ensure all required fields are filled
    if (targetPlaylistId == "" || targetPlaylistId == null) {
      setPlaylistError("Please Choose a Playlist");
      return;
    }
    // Create request object
    const request = [episode.id];

    // Send the request
    const response = await PlaylistHelper.playlistAddEpisodeRequest(
      request,
      targetPlaylistId,
    );

    if (response.status === 200) {
      onClose();
    } else {
      console.log(response.data);
      console.log(response.message);
      setPlaylistError(response.data);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>Adding {episode.episodeName} to a Playlist</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <VStack spacing={4}>
            <NexLink href={`/Playlist/MyPlaylists`}>
              <Button borderRadius={"50px"}>Create New Playlist</Button>
            </NexLink>
            <Text>OR</Text>
            {playlistError && <Text color="red.500">{playlistError}</Text>}
            {/* Dropdown to select an existing playlist */}
            <Select
              placeholder="Select an Existing Playlist"
              onChange={(e) => setTargetPlaylistId(e.target.value)}
            >
              {userPlaylists.map((playlist) => (
                <option key={playlist.id} value={playlist.id}>
                  {playlist.name}
                </option>
              ))}
            </Select>
          </VStack>
        </ModalBody>

        <ModalFooter>
          <Button colorScheme="red" mr={3} onClick={onClose}>
            Cancel
          </Button>
          <Button
            colorScheme="blue"
            mr={3}
            onClick={() => handleAddToExistingPlaylist()}
          >
            Add to Playlist
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};

export default AddToPlaylistModal;