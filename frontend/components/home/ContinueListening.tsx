import { Box, Text, VStack, Image, Flex, Progress, useColorModeValue } from "@chakra-ui/react";

const ContinueListening = () => {
  // Sample data for the continue listening section
  const episodes = [
    {
      thumbnail: "https://images.unsplash.com/photo-1533112435704-1a4bc513515d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1470&q=80",
      title: "Episode 1: Introduction to Web Development",
      progress: 60, // represents 60% listened
      description: "An introduction to the world of web development, covering the basics and advanced techniques.",
    },
    {
      thumbnail: "https://images.unsplash.com/photo-1533112435704-1a4bc513515d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1470&q=80",
      title: "Episode 2: Advanced JavaScript",
      progress: 30, // represents 30% listened
      description: "Dive deep into advanced JavaScript topics and explore the intricacies of the language.",
    },
    {
      thumbnail: "https://images.unsplash.com/photo-1533112435704-1a4bc513515d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1470&q=80",
      title: "Episode 3: Advanced JavaScript",
      progress: 30, // represents 30% listened
      description: "Dive deep into advanced JavaScript topics and explore the intricacies of the language.",
    },
    // ... Add more episodes as needed
  ];

  return (
    <VStack align="start" spacing={5} p={4} m={3} width="100%" flex="1" borderRadius="25px" backdropFilter="blur(35px)" boxShadow="xl" bg={useColorModeValue("rgba(255, 255, 255, 0.1)", "rgba(0, 0, 0, 0.1)")}>
      <Text fontSize="xl" fontWeight="bold" ml={3} position="sticky">
        Continue Listening
      </Text>
      {episodes.map((episode, index) => (
        <Flex key={index} mb={5} direction={{ base: "column", md: "row" }} alignItems="center">
          <Image boxSize={{ base: "100px", md: "100px" }} src={episode.thumbnail} alt={episode.title} mr={4} borderRadius="25px" />
          <Box flex="1">
            <Text fontWeight="bold" mb={2}>
              {episode.title}
            </Text>
            <Progress value={episode.progress} size="xs" colorScheme="teal" mb={2} />
            <Text fontSize="sm" color="gray.600">
              {episode.description}
            </Text>
          </Box>
        </Flex>
      ))}
    </VStack>
  );
};

export default ContinueListening;
