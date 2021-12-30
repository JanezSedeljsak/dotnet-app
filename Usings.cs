global using System;
global using System.Text;
global using System.Linq;
global using System.Security.Claims;
global using System.IdentityModel.Tokens.Jwt;
global using System.ComponentModel.DataAnnotations;
global using System.Collections.Generic;

global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization; 
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Mvc;

global using Core.AuthRepositoryWrapper;
global using Core.DataRepositoryWrapper;
global using Core.ContextWrapper;
global using Core.Models;
global using Core.Interfaces;

global using Core.Services.Translations;
global using Core.Services.TokenServiceWrapper;
global using Core.Services.PDFGenerator;

// added pdf services
global using DinkToPdf;
global using DinkToPdf.Contracts;