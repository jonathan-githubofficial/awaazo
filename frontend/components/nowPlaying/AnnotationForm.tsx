import React, { useState } from 'react';
import { Button, FormControl, FormLabel, Input, VStack, Select } from '@chakra-ui/react';
import AnnotationHelper from '../../helpers/AnnotationHelper';

const AnnotationForm = ({ episodeId, fetchAnnotations }) => {
  const [formData, setFormData] = useState({
    timestamp: '',
    content: '',
    videoUrl: '',
    platformType: '',
    name: '',
    website: '',
    annotationType: 'basic'
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async () => {
    const basePayload = {
      timestamp: Number(formData.timestamp),
      content: formData.content,
    };
  
    let payload;
    let response;
    try {
      let response;
      switch (formData.annotationType) {
        case 'mediaLink':
          payload = { ...basePayload, url: formData.videoUrl, platformType: formData.platformType };
          response = await AnnotationHelper.mediaLinkAnnotationCreateRequest(payload, episodeId);
          break;
        case 'sponsor':
          payload = { ...basePayload, name: formData.name, website: formData.website };
          response = await AnnotationHelper.sponsorAnnotationCreateRequest(payload, episodeId);
          break;
        default: // 'basic'
          payload = { ...basePayload, annotationType: 'info' }; // Adjust this if 'info' is not the correct type
          response = await AnnotationHelper.annotationCreateRequest(payload, episodeId);
          console.log("API Response:", response);
          break;
      }
      if (response && response.status === 200) {
        console.log("Annotation Creation Response:", response);
        fetchAnnotations(); 
      }
    } catch (error) {
      console.error("Error in creating Annotation:", error);
    }
    
  };
  

  return (
    <VStack spacing={4} align="stretch">
      <FormControl>
        <FormLabel>Annotation Type</FormLabel>
        <Select name="annotationType" value={formData.annotationType} onChange={handleChange}>
          <option value="basic">Basic</option>
          <option value="mediaLink">Media Link</option>
          <option value="sponsor">Sponsor</option>
        </Select>
      </FormControl>
      <FormControl>
        <FormLabel>Timestamp</FormLabel>
        <Input name="timestamp" value={formData.timestamp} onChange={handleChange} />
      </FormControl>
      <FormControl>
        <FormLabel>Content</FormLabel>
        <Input name="content" value={formData.content} onChange={handleChange} />
      </FormControl>
      {formData.annotationType === 'mediaLink' && (
        <>
          <FormControl>
            <FormLabel>Video URL</FormLabel>
            <Input name="videoUrl" value={formData.videoUrl} onChange={handleChange} />
          </FormControl>
          <FormControl>
            <FormLabel>Platform Type</FormLabel>
            <Input name="platformType" value={formData.platformType} onChange={handleChange} />
          </FormControl>
        </>
      )}
      {formData.annotationType === 'sponsor' && (
        <>
          <FormControl>
            <FormLabel>Name</FormLabel>
            <Input name="name" value={formData.name} onChange={handleChange} />
          </FormControl>
          <FormControl>
            <FormLabel>Website</FormLabel>
            <Input name="website" value={formData.website} onChange={handleChange} />
          </FormControl>
        </>
      )}
      <Button colorScheme="blue" onClick={handleSubmit}>Create Annotation</Button>
    </VStack>
  );
};

export default AnnotationForm;