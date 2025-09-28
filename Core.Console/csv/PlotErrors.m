%% Define colormap
colors = [
    153 255 51
    0 247 115
    0 234 176
    0 217 232
    0 197 255
    0 172 255
    0 142 255
    51 102 255
    155 81 236
    210 53 207
    246 1 172
    255 0 134
    255 24 96
    255 68 57
    255 102 0
    ];

%% Repackage data
newT = [];
newX = [];
newE = [];
C = [];
I = size(errors,1);
J = size(errors,2);
for i = 1:(I*J)
    row = mod(i,I)+1;
    col = mod(i,J)+1;
    newT(i) = Ts(row,col);
    newX(i) = xVs(row,col);
    newE(i) = errors(row,col);
end

%% Plot data
figure
scatter(newX', newT', 100, newE', '.')
xlabel('vapor composition [mol% n-propanol]')
ylabel('Temperature [K]')
cb = colorbar();
cb.Label.String = 'error [J/mol]';
zlabel('error')
colormap(colors./255)