import React, { useState } from "react";
import { Box, Flex, useBreakpointValue } from "@chakra-ui/react";
import Navbar from "../components/shared/Navbar";
import PlayerBar from "../components/shared/PlayerBar";
import ChatBot from "../components/player/ChatBot";
import Bookmarks from "../components/player/Bookmarks";
import Transcripts from "../components/player/Transcripts";
import { episodes } from "../utilities/SampleData";
import { AnyNaptrRecord } from "dns";

const Player: React.FC = () => {
  const direction = useBreakpointValue({ base: "column", md: "row" });
  const [selectedComponent, setSelectedComponent] = useState<number | null>(null);
  // Flags to show/hide components. Adjust these as needed.
  const showChatBot = true;
  const showBookmarks = true;
  const showTranscripts = true;

  const components = [
    {
      component: <ChatBot />,
      isVisible: showChatBot,
    },
    {
      component: <Bookmarks bookmarks={[
        {
          title: "This point was interesting",
          timestamp: "03:02",
          content: "This was good because ...",
        },
        {
          title: "Cool point they mentioned",
          timestamp: "05:44",
          content: "Lorem ipsum dolor sit amet...",
        },
      ]} />,
      isVisible: showBookmarks,
    },
    {
      component: <Transcripts timestamp={135} text="Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." />,
      isVisible: showTranscripts,
    },
    {
      component: <Box bg="red" w="full" h="full" />,  // Placeholder for the cover/description component
      isVisible: true,
    },
    // Add more components as needed
  ];

  const visibleComponents = components.filter(comp => comp.isVisible);
  
  const handleClick = (index: number) => {
    setSelectedComponent(index);
  };
  return (
    <Box w="100vw" h="100vh" display="flex" flexDirection="column">
      <Navbar />

      <Flex
        flexGrow={1}
        flexDirection={direction as any}
        mt="4"
        mx="4"
        justifyContent="space-between"
        align="center"
        flexWrap="wrap"
      >
        {visibleComponents.map((comp, index) => (
          <Box
            key={index}
            flex={selectedComponent === index ? 0.5 : 0.25}
            w="full"
            h="full"
            mb={{ base: "4", md: "0" }}
            p="2"
            transition="all 0.4s ease"
            onClick={() => handleClick(index)}
            boxSizing="border-box"
            maxH="calc((100vh - NavbarHeight - PlayerBarHeight)/numberOfComponents)"
          >
            {comp.component}
          </Box>
        ))}
      </Flex>

      <PlayerBar {...episodes[0]} />
    </Box>
  );
};

export default Player;