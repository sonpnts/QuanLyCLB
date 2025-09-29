using System;

namespace QuanLyCLB.Application.DTOs;

public record BranchDto(
    Guid Id,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters,
    string? GooglePlaceId,
    string? GoogleMapsEmbedUrl
);

public record CreateBranchRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters,
    string? GooglePlaceId,
    string? GoogleMapsEmbedUrl
);

public record UpdateBranchRequest(
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    double AllowedRadiusMeters,
    string? GooglePlaceId,
    string? GoogleMapsEmbedUrl,
    bool IsActive
);
