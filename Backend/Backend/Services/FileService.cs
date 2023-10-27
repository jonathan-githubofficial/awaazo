﻿using Backend.Infrastructure;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace Backend.Services
{
    public class FileService : IFileService
    {

        private readonly AppDbContext _db;
        public FileService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Guid?> SaveFile(string Name, string MimeType)
        {
            var uploadFile = new Files();
            uploadFile.Name = Name;
            uploadFile.MimeType = MimeType;
            await _db.File!.AddAsync(uploadFile);
            await _db.SaveChangesAsync();
            return uploadFile.FileId;
        }

        public async Task<bool> CleanUp(Guid? guid)
        {
            Files? f1 = _db.File!.SingleOrDefault(x => x.FileId == guid);
            if (f1 != null)
            {
                _db.File!.Remove(f1);
                await _db.SaveChangesAsync();
                return true;

            }
            return false;

        }

        public string GetPath(string path)
        {
            return Path.Combine(AppContext.BaseDirectory, "DEFAULT", path);
        }


        public bool Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;


            }
            return false;

        }


        public async Task<bool?> EditFile(Files f1 ,IFormFile file)
        {
            
            if(f1 != null)
            {
                bool? delete = await DeleteFile(f1.FileId.ToString()!);
                if(delete == true)
                {
                    string dirName = "DEFAULT";
                    string dirPath = Path.Combine(AppContext.BaseDirectory, dirName);
                    string filePath = Path.Combine(dirPath, f1.FileId.ToString()! + "." + file.ContentType.Split("/")[1]);
                    using FileStream fs = new(filePath, FileMode.Create);
                    file.CopyTo(fs);
                    f1.Name = file.FileName;
                    f1.MimeType = file.ContentType;
                    return true;

                }
                return false;


            }
            return false;

        }
       
        public async Task<bool?> DeleteFile(string id)
        {
            Guid? guid = Guid.Parse(id);


            Files? file = await  _db.File!.FirstOrDefaultAsync(u => u.FileId == guid);
            if (file != null)
            {
                if(Delete(GetPath(file.Path!)))
                {

                    return true;
                }
                else
                {
                    throw new Exception("Error Deleteing the file");
                }
            }
            else
            {
            throw new Exception($"Could not Find the file {id}");

            }

        }
        public async Task<Files?> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {

                Guid? guid = await SaveFile(file.FileName, file.ContentType);

                if (guid != null)
                {
                    string dirName = "DEFAULT";
                    string dirPath = Path.Combine(AppContext.BaseDirectory, dirName);

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    // Get the file path
               
                    string filePath = Path.Combine(dirPath, guid.ToString() + "." + file.ContentType.Split("/")[1]);

                    // Save the file
                    using FileStream fs = new(filePath, FileMode.Create);
                    file.CopyTo(fs);

                    // Return true if the file was saved successfully

                    if (File.Exists(filePath))
                    {
                        Files? f1 = _db.File!.SingleOrDefault(u => u.FileId == guid);
                        if (f1 != null)
                        {
                            f1.Path = guid.ToString() + "." + file.ContentType.Split("/")[1] ;
                            await _db.SaveChangesAsync();

                        }
                        return f1;

                    }
                    else
                    {
                        await CleanUp(guid);
                        return null;


                    }

                }
                return null;
            }
            return null;

        }

    }
}