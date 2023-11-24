interface LoginRequestDto {
    username: string;
    password: string;
    origin: string;
}

interface LoginResponseDto {
    id: number,
    userName: string,
    role: string,
    accessToken: string,
}