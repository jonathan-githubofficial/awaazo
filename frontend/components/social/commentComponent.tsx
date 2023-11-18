import { useState, useEffect } from "react";
import SocialHelper from "../../helpers/SocialHelper";
import PodcastHelper from "../../helpers/PodcastHelper";
import {
  Button,
  Modal,
  ModalOverlay,
  ModalContent,
  ModalHeader,
  ModalCloseButton,
  ModalBody,
  Textarea,
  VStack,
  useDisclosure,
  Icon,
  Avatar,
  Text,
  HStack,
  Box,
  Tooltip,
  Input,
  IconButton,
} from "@chakra-ui/react";
import {
  FaComments,
  FaClock,
  FaPaperPlane,
  FaHeart,
  FaReply,
} from "react-icons/fa";
import { Comment } from "../../utilities/Interfaces";

// CommentComponent is a component that displays comments and allows users to add new comments, reply to comments, and like/unlike comments
const CommentComponent = ({
  episodeIdOrCommentId,
  initialLikes,
  initialIsLiked,
}) => {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState("");
  const [replyText, setReplyText] = useState("");
  const [isLiked, setIsLiked] = useState(initialIsLiked);
  const [noOfComments, setNoOfComments] = useState(initialLikes);

  // Fetch episode details and transform comments when the modal is opened
  useEffect(() => {
    if (isOpen) {
      const fetchEpisodeDetails = async () => {
        try {
          const response = await PodcastHelper.getEpisodeById(
            episodeIdOrCommentId
          );
          if (response.status === 200) {
            if (response.episode) {
              // Transform the comments to match your expected format
              const transformedComments = response.episode.comments.map(
                (comment) => ({
                  id: comment.id,
                  episodeId: comment.episodeId,
                  user: comment.user,
                  dateCreated: new Date(comment.dateCreated),
                  text: comment.text,
                  likes: comment.likes,
                  replies: comment.replies,
                })
              );
              setComments(transformedComments);
            }
          } else {
            console.error("Error fetching episode details:", response.message);
          }
        } catch (error) {
          console.error("Error fetching episode details:", error.message);
        }
      };

      fetchEpisodeDetails();
    }
  }, [isOpen, episodeIdOrCommentId]);

  // Add a new comment
  const handleAddComment = async () => {
    if (newComment.trim()) {
      const response = await SocialHelper.postEpisodeComment(
        newComment,
        episodeIdOrCommentId
      );
      if (response.status === 200) {
        // Update the UI to reflect the new comment
        const request = {
          text: newComment,
        };
        //to fix
        //setComments((comments) => [...comments, newComment]);
      } else {
        console.error("Error posting comment:", response.message);
      }
      setNewComment("");
    }
  };

  // Reply to a comment
  const handleReply = async (index: number) => {
    const comment = comments[index];
    const commentId = comment.id; // Assuming each comment has a unique 'id' property

    const updatedComments = [...comments];
    //updatedComments[index].replies.push(replyText);
    setComments(updatedComments);
    const response = await SocialHelper.postEpisodeComment(
      replyText,
      commentId
    );
    if (response.status === 200) {
      // Update the UI to reflect the new comment
      //to fix
      //setComments((comments) => [...comments, newComment]);
    } else {
      console.error("Error posting comment:", response.message);
    }
    setReplyText("");
  };

  // Like/unlike a comment
  const handleLike = (index: number) => {
    const comment = comments[index];
    const commentId = comment.id; // Assuming each comment has a unique 'id' property

    // Toggle the like status based on whether the comment is currently liked
    if (isLiked) {
      // Call unlikeComment because the comment is currently liked
      SocialHelper.deleteEpisodeLike(commentId)
        .then((response) => {
          if (response.status === 200) {
            // Update the UI to reflect the unlike
            setNoOfComments(noOfComments - 1);
            setIsLiked(false);
          } else {
            console.error("Error unliking comment:", response.message);
          }
        })
        .catch((error) => {
          console.error("Exception when calling unlikeComment:", error.message);
        });
    } else {
      // Call likeComment because the comment is currently not liked
      SocialHelper.postEpisodeLike(commentId)
        .then((response) => {
          if (response.status === 200) {
            // Update the UI to reflect the like
            setNoOfComments(noOfComments + 1);
            setIsLiked(true);
          } else {
            console.error("Error liking comment:", response.message);
          }
        })
        .catch((error) => {
          console.error("Exception when calling likeComment:", error.message);
        });
    }
  };

  const handleDeleteComment = (commentId) => {
    SocialHelper.deleteComment(commentId)
      .then((response) => {
        if (response.status === 200) {
          setComments((prevComments) =>
            prevComments.filter((comment) => comment.id !== commentId)
          );
        } else {
          console.error("Error deleting comment:", response.message);
        }
      })
      .catch((error) => {
        // Handle any errors that occur during the deletion process
        console.error("Exception when calling deleteComment:", error.message);
      });
  };

  return (
    <>
      <Tooltip label="Post a comment" aria-label="Comment tooltip">
        <Button
          p={2}
          m={1}
          leftIcon={<Icon as={FaComments} />}
          onClick={onOpen}
          variant={"ghost"}
        >
          {noOfComments}
        </Button>
      </Tooltip>

      <Modal isOpen={isOpen} onClose={onClose} size="xl">
        <ModalOverlay />
        <ModalContent
          boxShadow="dark-lg"
          backdropFilter="blur(40px)"
          display="flex"
          flexDirection="column"
          justifyContent="center"
          alignItems="center"
          alignSelf={"center"}
          padding={"2em"}
          backgroundColor="rgba(255, 255, 255, 0.1)"
          borderRadius={"2em"}
          outlineColor="rgba(255, 255, 255, 0.25)"
        >
          <ModalHeader fontWeight={"light"} fontSize={"1.5em"}>
            Comments
          </ModalHeader>
          <ModalCloseButton />
          <ModalBody>
            <VStack spacing={5} align="start" height="300px" overflowY="auto">
              {comments.length > 0 ? (
                comments.map((comment, index) => (
                  <Box
                    key={index}
                    p={3}
                    borderRadius="md"
                    boxShadow="sm"
                    bg={index % 2 === 0 ? "gray.550" : "gray.600"}
                    _hover={{ bg: "gray.400", transition: "0.3s" }}
                    width="100%"
                  >
                    <HStack spacing={5}>
                      <Avatar src={comment.user.avatarUrl} />
                      <VStack align="start" spacing={1} flex="1">
                        <Text fontWeight="bold" isTruncated>
                          {comment.user.username}
                        </Text>
                        <Text isTruncated>{comment.text}</Text>
                      </VStack>
                    </HStack>
                    <HStack spacing={1} p={2} borderRadius="md">
                      <Icon as={FaClock} color="gray.500" />
                      <Text fontSize="xs" color="gray.500">
                        {comment.dateCreated.toLocaleString()}
                      </Text>
                    </HStack>
                    <HStack mt={3} spacing={2}>
                      <Tooltip
                        label={
                          comment.likes
                            ? "Unlike this comment"
                            : "Like this comment"
                        }
                        aria-label="Like tooltip"
                      >
                        <IconButton
                          icon={
                            <Icon
                              as={FaHeart}
                              color={comment.likes ? "red.500" : "gray.500"}
                            />
                          }
                          onClick={() => handleLike(index)}
                          aria-label="Like Comment"
                          size="sm"
                          backgroundColor={"transparent"}
                        />
                      </Tooltip>
                      <Text fontSize="sm">{comment.likes.length}</Text>
                    </HStack>
                    <VStack align="start" spacing={2} mt={3} pl={8}>
                      {comment.replies.map((reply, index) => (
                        <Box key={index} bg="gray.650" p={2} borderRadius="md">
                          <Avatar src={reply.user.avatarUrl} />
                          <Text fontWeight="bold">{reply.user.username}:</Text>
                          <Text>{reply.text}</Text>
                          <HStack spacing={1} p={2} borderRadius="md">
                            <Icon as={FaClock} color="gray.500" />
                            <Text fontSize="xs" color="gray.500">
                              {new Date(reply.dateCreated).toLocaleString()}
                            </Text>
                          </HStack>
                        </Box>
                      ))}
                      <Box mt={2}>
                        <HStack spacing={2}>
                          <Input
                            flex="1"
                            placeholder="Reply to this comment..."
                            value={replyText}
                            onChange={(e) => setReplyText(e.target.value)}
                          />
                          <Tooltip
                            label="Reply to this comment"
                            aria-label="Reply tooltip"
                          >
                            <IconButton
                              icon={<Icon as={FaReply} />}
                              onClick={() => handleReply(index)}
                              aria-label="Reply to Comment"
                              size="sm"
                            />
                          </Tooltip>
                        </HStack>
                      </Box>
                    </VStack>
                  </Box>
                ))
              ) : (
                <Text color="gray.500" alignSelf={"center"}>
                  No comments yet. Be the first!
                </Text>
              )}
            </VStack>
            <VStack position={"relative"}>
              <Textarea
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                placeholder="Add a comment..."
                borderRadius={"1em"}
              />
              <Button
                leftIcon={<Icon as={FaPaperPlane} />}
                colorScheme="blue"
                onClick={handleAddComment}
                zIndex="1"
                fontSize="md"
                borderRadius={"full"}
                minWidth={"10em"}
                color={"white"}
                marginTop={"15px"}
                marginBottom={"10px"}
                padding={"20px"}
                // semi transparent white outline
                outline={"1px solid rgba(255, 255, 255, 0.6)"}
                style={{
                  background:
                    "linear-gradient(45deg, #007BFF, #3F60D9, #5E43BA, #7C26A5, #9A0A90)",
                  backgroundSize: "300% 300%",
                  animation: "Gradient 10s infinite linear",
                }}
              >
                Add Comment
              </Button>
            </VStack>
          </ModalBody>
        </ModalContent>
      </Modal>
    </>
  );
};

export default CommentComponent;