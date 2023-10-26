﻿using Backend.Controllers.Requests;
using Backend.Infrastructure;
using Backend.Models;
using Backend.Services;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Backend.Controllers
{
    [ApiController]
    [Route("podcast")]
    public class PodcastController : ControllerBase
    {
        private readonly IPodcastService _podcastService;
        private readonly IFileService _fileService;
        private readonly AppDbContext _db;
        public PodcastController(IPodcastService podcastService,AppDbContext db,IFileService fileService)
        {
            _podcastService = podcastService;
            _fileService = fileService;
            _db = db;
        }
        //TODO : ADD middleware to Validate the File type inputted
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreatePodcast([FromForm]CreatePodcastRequest createPodcastRequest)
        {
            
            GetPodcastRequest? podcast = await _podcastService.CreatePodcast(createPodcastRequest, HttpContext);
            

            if (podcast != null)
            {

            return Ok(podcast);
            
            }
            else { return BadRequest("Bad Request"); }

        }
        [HttpGet("getById")]
        [Authorize]
        public async Task<IActionResult> GetPodcastById(string id)
        {
            Podcast? podcast =  await _podcastService.GetPodcast(id);
            if(podcast != null)
            {
                return Ok(podcast);
            }
            else
            {
                return BadRequest("Bad Request");
            }
        
        }

        [HttpGet("Cover")]
        [Authorize]
        public async Task<PhysicalFileResult> GetCoverById(string id)
        {
            Guid guid = Guid.Parse(id);
            Files? file = await _db.File!.FirstOrDefaultAsync(u => u.FileId == guid);
           
            
            return PhysicalFile(_fileService.GetPath(file!.Path!,"COVER"), file!.MimeType);

           
            
        }
        }


       






        
    }

